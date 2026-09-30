using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using StationSales.Application;
using StationSales.Domain;

namespace StationSales.Infrastructure;

public sealed class DeterministicSha256HashService : IHashService
{
    public byte[] Compute(params object?[] values) => SHA256.HashData(Encoding.UTF8.GetBytes(string.Join("\u001f", values.Select(Canonical))));
    private static string Canonical(object? value) => value switch { null => "<NULL>", byte[] x => Convert.ToBase64String(x), DateTime x => x.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture), decimal x => x.ToString(CultureInfo.InvariantCulture), _ => value.ToString()!.Trim() };
}
public sealed class ConfigurationDataSourceSecretResolver : IDataSourceSecretResolver
{
    private readonly IConfiguration _configuration;
    public ConfigurationDataSourceSecretResolver(IConfiguration configuration) => _configuration = configuration;
    public DataSourceCredential Resolve(string secretReference)
    {
        if (string.IsNullOrWhiteSpace(secretReference)) throw new InvalidOperationException("Data source has no SecretReference.");
        var section = _configuration.GetSection($"DataSourceSecrets:{secretReference}");
        var username = section["UserName"]; var password = section["Password"];
        return !string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password)
            ? new DataSourceCredential(username, password)
            : throw new InvalidOperationException($"Credential '{secretReference}' is missing from application configuration.");
    }
}
public sealed class ExtractorResolver : IExtractorResolver
{
    private readonly IReadOnlyDictionary<SourceProvider, IStationSaleExtractor> _items;
    public ExtractorResolver(IEnumerable<IStationSaleExtractor> extractors) => _items = extractors.ToDictionary(x => x.Provider);
    public IStationSaleExtractor Resolve(SourceProvider provider) => _items.TryGetValue(provider, out var extractor) ? extractor : throw new NotSupportedException($"No extractor registered for {provider}.");
}
public abstract class ProviderExtractorBase : IStationSaleExtractor
{
    public abstract SourceProvider Provider { get; }
    public virtual bool ProvidesCompleteDocumentSnapshot => false;
    public Task<IReadOnlyCollection<ExtractedSaleRow>> ExtractAsync(DataSource source, ExtractionWindow window, CancellationToken cancellationToken) => throw new NotImplementedException($"TODO: configure the real {Provider} sales query and raw DTO contract before extraction can run.");
}
public sealed class EfExtractionRepository : IExtractionRepository
{
    private readonly StationSalesDbContext _db; private readonly IClock _clock; public EfExtractionRepository(StationSalesDbContext db, IClock clock) => (_db, _clock) = (db, clock);
    public async Task<IReadOnlyCollection<PipelineDataSource>> GetActiveAsync(string? sourceCode, CancellationToken token) => await _db.PipelineDataSources.Include(x => x.DataSource).Include(x => x.Pipeline).Where(x => x.IsActive && x.Pipeline.IsActive && x.Pipeline.Code == "EXTRACT_STATION_SALES" && x.DataSource.IsActive && (sourceCode == null || x.DataSource.Code == sourceCode)).ToListAsync(token);
    public async Task<ExtractionRun> StartAsync(PipelineDataSource config, ExtractionWindow window, CancellationToken token) { var run = new ExtractionRun { PipelineDataSourceId = config.Id, DataSourceId = config.DataSourceId, WindowFrom = window.FromUtc, WindowTo = window.ToUtc, StartedAtUtc = _clock.UtcNow, CreatedAtUtc = _clock.UtcNow }; _db.ExtractionRuns.Add(run); await _db.SaveChangesAsync(token); return run; }
    public async Task SucceedAsync(ExtractionRun run, PipelineDataSource config, int read, int written, DateTime checkpoint, CancellationToken token) { run.Succeed(read, written, _clock.UtcNow); config.ConfirmCheckpoint(checkpoint); await _db.SaveChangesAsync(token); }
    public async Task SucceedWithoutCheckpointAsync(ExtractionRun run, int read, int written, CancellationToken token) { run.Succeed(read, written, _clock.UtcNow); await _db.SaveChangesAsync(token); }
    public async Task SucceedWithNoDataAsync(ExtractionRun run, CancellationToken token) { run.SucceedWithNoData(_clock.UtcNow); await _db.SaveChangesAsync(token); }
    public async Task FailAsync(ExtractionRun run, Exception exception, CancellationToken token) { run.Fail(exception.GetType().Name, exception.Message[..Math.Min(exception.Message.Length, 1000)], _clock.UtcNow); await _db.SaveChangesAsync(token); }
}
public sealed class EfRawSaleWriter : IRawSaleWriter
{
    private const int MaxDeadlockAttempts = 3;
    private const int ApplicationLockTimeoutMilliseconds = 120_000;
    private readonly StationSalesDbContext _db;
    private readonly IClock _clock;
    private readonly ILogger<EfRawSaleWriter> _logger;
    public EfRawSaleWriter(StationSalesDbContext db, IClock clock, ILogger<EfRawSaleWriter> logger) => (_db, _clock, _logger) = (db, clock, logger);
    public async Task<int> WriteAsync(ExtractionRun run, DataSource source, IReadOnlyCollection<ExtractedSaleRow> rows, CancellationToken token)
    {
        if (source.Provider == SourceProvider.OpenComb) return await UpsertOpenCombAsync(run, source, rows, token);
        if (source.Provider == SourceProvider.Gasolution) return await UpsertGasolutionAsync(run, source, rows, token);
        throw new NotSupportedException($"Unsupported raw provider '{source.Provider}'.");
    }
    private async Task<int> UpsertGasolutionAsync(ExtractionRun run, DataSource source, IReadOnlyCollection<ExtractedSaleRow> rows, CancellationToken token)
    {
        ValidateDistinctSourceRecords(rows, source.Code);
        var batch = rows.Select(row => { var target = new GasolutionStationSaleRaw(); ApplyGasolution(target, run, source, row); return target; }).ToList();
        await BulkUpsertAsync(batch, "gasolution_station_sale", "gasolution_station_sale_staging", token);
        return rows.Count;
    }
    private void ApplyGasolution(GasolutionStationSaleRaw target, ExtractionRun run, DataSource source, ExtractedSaleRow row)
    {
        target.ExtractionRunId = run.Id; target.DataSourceId = source.Id; target.StationId = source.StationId; target.StationCode = source.StationCode; target.SourceRecordId = row.SourceRecordId; target.SourceUpdatedAtUtc = row.SourceUpdatedAtUtc; target.ExtractedAtUtc = _clock.UtcNow; target.RowHash = RowHash(row);
        target.SourceDatabase = Text(row, "source_database"); target.Recibo = Long(row, "Recibo"); target.Ruc = Text(row, "RUC"); target.RazonSocial = Text(row, "RazonSocial"); target.FormaPago = Text(row, "Descripcion"); target.FechaHora = Date(row, "FechaHora"); target.HoraInicio = Date(row, "HoraInicio"); target.HoraFin = Date(row, "HoraFin"); target.NumeroTurno = Int(row, "NumeroTurno"); target.Cedula = Text(row, "Cedula"); target.Despachador = Text(row, "Despachador"); target.Placa = Text(row, "Placa"); target.Manguera = Text(row, "Manguera"); target.Surtidor = Text(row, "Surtidor"); target.Cara = Text(row, "Cara");
        target.LecturaInicial = Decimal(row, "LecturaInicial"); target.LecturaFinal = Decimal(row, "LecturaFinal"); target.Precio = Decimal(row, "Precio"); target.Cantidad = Decimal(row, "Cantidad"); target.Producto = Text(row, "Producto"); target.Valor = Decimal(row, "Valor"); target.Descuento = Decimal(row, "Descuento"); target.Recaudo = Decimal(row, "Recaudo"); target.EsDescuentoClienteCredito = Bool(row, "EsDescuentoClienteCredito"); target.ValorCambioPrecio = Decimal(row, "ValorCambioPrecio"); target.TipoRedondeo = Int(row, "TipoRedondeo"); target.ValorRedondeo = Decimal(row, "ValorRedondeo"); target.NroConfirmacionCdc = Long(row, "NroConfirmacionCDC"); target.IdVentaFacturacion = Long(row, "IdVentaFacturacion");
        target.TipoDocumento = Text(row, "TipoDocumento"); target.CodigoSunat = Text(row, "CodigoSunat"); target.Serie = Text(row, "Serie"); target.Numero = Text(row, "Numero"); target.FechaHoraDocumento = Date(row, "FechaHoraDocumento"); target.PrecioDocumento = Decimal(row, "PrecioDocumento"); target.BaseImponibleDocumento = Decimal(row, "BaseImponibleDocumento"); target.ImpuestoDocumento = Decimal(row, "ImpuestoDocumento"); target.SubtotalDocumento = Decimal(row, "SubtotalDocumento"); target.TotalPagarDocumento = Decimal(row, "TotalPagarDocumento"); target.RedondeoDonacionDocumento = Decimal(row, "RedondeoDonacionDocumento"); target.CodigoQr = Text(row, "CodigoQr"); target.TramaEnviada = Text(row, "TramaEnviada");
    }
    private async Task<int> UpsertOpenCombAsync(ExtractionRun run, DataSource source, IReadOnlyCollection<ExtractedSaleRow> rows, CancellationToken token)
    {
        ValidateDistinctSourceRecords(rows, source.Code);
        var batch = rows.Select(row => { var target = new OpenCombStationSaleRaw(); ApplyOpenComb(target, run, source, row); return target; }).ToList();
        await BulkUpsertAsync(batch, "opencomb_station_sale", "opencomb_station_sale_staging", token);
        return rows.Count;
    }
    private void ApplyOpenComb(OpenCombStationSaleRaw target, ExtractionRun run, DataSource source, ExtractedSaleRow row)
    {
        target.ExtractionRunId = run.Id; target.DataSourceId = source.Id; target.StationId = source.StationId; target.StationCode = source.StationCode; target.SourceRecordId = row.SourceRecordId; target.SourceUpdatedAtUtc = row.SourceUpdatedAtUtc; target.ExtractedAtUtc = _clock.UtcNow; target.RowHash = RowHash(row);
        target.SourceTable = Text(row, "source_table"); target.ArtDescripcion = Text(row, "art_descripcion"); target.RazonSocial = Text(row, "razon_social"); target.Ruc = Text(row, "ruc"); target.Tm = Text(row, "tm"); target.Caja = Text(row, "caja"); target.Td = Text(row, "td"); target.Dia = Date(row, "dia"); target.Turno = Text(row, "turno"); target.Grupo = Text(row, "grupo"); target.Codigo = Text(row, "codigo"); target.Cantidad = Decimal(row, "cantidad"); target.Precio = Decimal(row, "precio"); target.Igv = Decimal(row, "igv"); target.Importe = Decimal(row, "importe"); target.Cajero = Text(row, "cajero"); target.Fecha = Date(row, "fecha"); target.Tipo = Text(row, "tipo"); target.Pump = Text(row, "pump"); target.Fpago = Text(row, "fpago"); target.At = Text(row, "at"); target.Text1 = Text(row, "text1"); target.Tarjeta = Text(row, "tarjeta"); target.Km = Decimal(row, "km"); target.RendiGln = Decimal(row, "rendi_gln"); target.RendiAcu = Decimal(row, "rendi_acu"); target.SolesKm = Decimal(row, "soles_km"); target.Placa = Text(row, "placa"); target.Nombre = Text(row, "nombre"); target.Usr = Text(row, "usr"); target.Es = Text(row, "es"); target.FlgReplicacion = Int(row, "flg_replicacion"); target.FechaReplicacion = Date(row, "fecha_replicacion"); target.Trans = Decimal(row, "trans"); target.Ebidata = Text(row, "ebidata");
    }
    private static object? Value(ExtractedSaleRow row, string name) => row.Values.TryGetValue(name, out var value) ? value : null;
    private static string? Text(ExtractedSaleRow row, string name) => Value(row, name)?.ToString()?.Trim();
    private static DateTime? Date(ExtractedSaleRow row, string name) => Value(row, name) is DateTime value ? value : null;
    private static decimal? Decimal(ExtractedSaleRow row, string name) => Value(row, name) is null ? null : Convert.ToDecimal(Value(row, name), CultureInfo.InvariantCulture);
    private static int? Int(ExtractedSaleRow row, string name) => Value(row, name) is null ? null : Convert.ToInt32(Value(row, name), CultureInfo.InvariantCulture);
    private static long? Long(ExtractedSaleRow row, string name) => Value(row, name) is null ? null : Convert.ToInt64(Value(row, name), CultureInfo.InvariantCulture);
    private static bool? Bool(ExtractedSaleRow row, string name) => Value(row, name) is null ? null : Convert.ToBoolean(Value(row, name), CultureInfo.InvariantCulture);
    private static void ValidateDistinctSourceRecords(IReadOnlyCollection<ExtractedSaleRow> rows, string sourceCode)
    {
        var duplicate = rows.GroupBy(x => x.SourceRecordId, StringComparer.Ordinal).FirstOrDefault(x => x.Count() > 1);
        if (duplicate is not null) throw new InvalidOperationException($"La fuente '{sourceCode}' devolvió más de una fila para SourceRecordId '{duplicate.Key}'.");
    }
    private static byte[] RowHash(ExtractedSaleRow row) => SHA256.HashData(Encoding.UTF8.GetBytes(string.Join("\u001f", row.Values.OrderBy(x => x.Key, StringComparer.Ordinal).Select(x => $"{x.Key}={Canonical(x.Value)}"))));
    private static string Canonical(object? value) => value switch { null => "<NULL>", DateTime x => x.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture), decimal x => x.ToString(CultureInfo.InvariantCulture), IFormattable x => x.ToString(null, CultureInfo.InvariantCulture), _ => value.ToString()?.Trim() ?? "<NULL>" };
    private async Task BulkUpsertAsync<TEntity>(IReadOnlyCollection<TEntity> rows, string targetTable, string stagingTable, CancellationToken token) where TEntity : class
    {
        if (rows.Count == 0) return;
        var entityType = _db.Model.FindEntityType(typeof(TEntity))!;
        var table = StoreObjectIdentifier.Table(targetTable, "raw");
        var properties = entityType.GetProperties().Where(x => x.Name != "Id").ToArray();
        var columns = properties.Select(x => x.GetColumnName(table) ?? x.Name).ToArray();
        var data = new DataTable();
        foreach (var property in properties) data.Columns.Add(property.Name, Nullable.GetUnderlyingType(property.ClrType) ?? property.ClrType);
        foreach (var item in rows)
        {
            var record = data.NewRow();
            for (var i = 0; i < properties.Length; i++) record[i] = properties[i].PropertyInfo!.GetValue(item) ?? DBNull.Value;
            data.Rows.Add(record);
        }
        for (var attempt = 1; attempt <= MaxDeadlockAttempts; attempt++)
        {
            try
            {
                await BulkUpsertAttemptAsync(data, properties, columns, table, targetTable, stagingTable, token);
                return;
            }
            catch (SqlException ex) when (ex.Number == 1205 && attempt < MaxDeadlockAttempts)
            {
                var delay = TimeSpan.FromMilliseconds((200 * (1 << (attempt - 1))) + Random.Shared.Next(50, 201));
                _logger.LogWarning(ex, "Deadlock writing raw table {TargetTable}. Retrying attempt {NextAttempt}/{MaxAttempts} in {DelayMilliseconds} ms.", targetTable, attempt + 1, MaxDeadlockAttempts, delay.TotalMilliseconds);
                await Task.Delay(delay, token);
            }
        }
    }

    private async Task BulkUpsertAttemptAsync(DataTable data, IReadOnlyCollection<IProperty> properties, IReadOnlyCollection<string> columns, StoreObjectIdentifier table, string targetTable, string stagingTable, CancellationToken token)
    {
        var connection = (SqlConnection)_db.Database.GetDbConnection();
        await connection.OpenAsync(token);
        await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync(token);
        try
        {
            await AcquireApplicationLockAsync(connection, transaction, targetTable, token);

            await using var clear = connection.CreateCommand();
            clear.Transaction = transaction;
            clear.CommandText = $"DELETE FROM [raw].[{stagingTable}] WHERE [ExtractionRunId] = @runId";
            clear.Parameters.Add(new SqlParameter("@runId", data.Rows[0]["ExtractionRunId"]));
            await clear.ExecuteNonQueryAsync(token);

            using (var bulk = new SqlBulkCopy(connection, SqlBulkCopyOptions.TableLock, transaction) { DestinationTableName = $"[raw].[{stagingTable}]", BatchSize = 2_000 })
            {
                foreach (var property in properties) bulk.ColumnMappings.Add(property.Name, property.GetColumnName(table) ?? property.Name);
                await bulk.WriteToServerAsync(data, token);
            }

            var key = "t.[DataSourceId] = s.[DataSourceId] AND t.[SourceRecordId] = s.[SourceRecordId]";
            var assignments = string.Join(", ", columns.Where(x => x is not "DataSourceId" and not "SourceRecordId").Select(x => $"t.[{x}] = s.[{x}]"));
            var listed = string.Join(", ", columns.Select(x => $"[{x}]"));
            var selected = string.Join(", ", columns.Select(x => $"s.[{x}]"));
            await using var merge = connection.CreateCommand();
            merge.Transaction = transaction;
            merge.CommandText = $@"
UPDATE t SET {assignments}
FROM [raw].[{targetTable}] t INNER JOIN [raw].[{stagingTable}] s ON {key}
WHERE s.[ExtractionRunId] = @runId AND (t.[RowHash] IS NULL OR t.[RowHash] <> s.[RowHash]);
INSERT INTO [raw].[{targetTable}] ({listed})
SELECT {selected} FROM [raw].[{stagingTable}] s
WHERE s.[ExtractionRunId] = @runId AND NOT EXISTS (SELECT 1 FROM [raw].[{targetTable}] t WHERE {key});
DELETE FROM [raw].[{stagingTable}] WHERE [ExtractionRunId] = @runId;";
            merge.Parameters.Add(new SqlParameter("@runId", data.Rows[0]["ExtractionRunId"]));
            await merge.ExecuteNonQueryAsync(token);
            await transaction.CommitAsync(token);
        }
        catch
        {
            try { await transaction.RollbackAsync(CancellationToken.None); }
            catch (Exception rollbackException) { _logger.LogWarning(rollbackException, "Rollback failed after raw write error for table {TargetTable}.", targetTable); }
            throw;
        }
        finally { await connection.CloseAsync(); }
    }

    private static async Task AcquireApplicationLockAsync(SqlConnection connection, SqlTransaction transaction, string targetTable, CancellationToken token)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandTimeout = (ApplicationLockTimeoutMilliseconds / 1_000) + 30;
        command.CommandText = @"
DECLARE @result int;
EXEC @result = sys.sp_getapplock
    @Resource = @resource,
    @LockMode = 'Exclusive',
    @LockOwner = 'Transaction',
    @LockTimeout = @lockTimeout;
SELECT @result;";
        command.Parameters.Add(new SqlParameter("@resource", SqlDbType.NVarChar, 255) { Value = $"StationSales:raw:{targetTable}" });
        command.Parameters.Add(new SqlParameter("@lockTimeout", SqlDbType.Int) { Value = ApplicationLockTimeoutMilliseconds });
        var result = Convert.ToInt32(await command.ExecuteScalarAsync(token), CultureInfo.InvariantCulture);
        if (result < 0) throw new InvalidOperationException($"Could not acquire the central RAW write lock for table '{targetTable}'. sp_getapplock returned {result}.");
    }
}
public sealed class EfCoreRepository : ICoreRepository
{
    private readonly StationSalesDbContext _db; public EfCoreRepository(StationSalesDbContext db) => _db = db;
    public Task<SaleDocument?> FindDocumentAsync(byte[] key, CancellationToken token) => _db.SaleDocuments.Include(x => x.Details).SingleOrDefaultAsync(x => x.DocumentKeyHash == key, token);
    public Task AddDocumentAsync(SaleDocument document, CancellationToken token) { _db.SaleDocuments.Add(document); return Task.CompletedTask; }
    public Task SaveChangesAsync(CancellationToken token) => _db.SaveChangesAsync(token);
}
public sealed class SqlServerMartPublisher : IMartPublisher { public PublishDestination Destination => PublishDestination.SqlServer; public Task PublishAsync(CancellationToken cancellationToken) => Task.CompletedTask; }
public sealed class PostgreSqlMartPublisher : IMartPublisher { public PublishDestination Destination => PublishDestination.PostgreSql; public Task PublishAsync(CancellationToken cancellationToken) => Task.CompletedTask; }
