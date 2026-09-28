using System.Data;
using System.Globalization;
using Microsoft.Data.SqlClient;
using StationSales.Application;
using StationSales.Domain;

namespace StationSales.Infrastructure;

/// <summary>Reads Gasolution exclusively through dbo.RecuperarVentasEnergigas.</summary>
public sealed class GasolutionStationSaleExtractor : IStationSaleExtractor
{
    private const string MainDatabaseName = "Hadi";
    private const int CommandTimeoutSeconds = 300;
    private readonly IDataSourceSecretResolver _secrets;

    public GasolutionStationSaleExtractor(IDataSourceSecretResolver secrets) => _secrets = secrets;
    public SourceProvider Provider => SourceProvider.Gasolution;
    public bool ProvidesCompleteDocumentSnapshot => false;

    public async Task<IReadOnlyCollection<ExtractedSaleRow>> ExtractAsync(DataSource source, ExtractionWindow window, CancellationToken cancellationToken)
    {
        if (!source.DatabaseName.Equals(MainDatabaseName, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"La fuente Gasolution '{source.Code}' debe tener '{MainDatabaseName}' como base de datos principal.");

        var credential = _secrets.Resolve(source.SecretReference);
        var localStart = ToLimaDate(window.FromUtc);
        var localEnd = ToLimaDate(window.ToUtc);
        var rows = new Dictionary<string, ExtractedSaleRow>(StringComparer.Ordinal);

        // Hadi is mandatory: connectivity, execution, and data-contract failures must fail the extraction.
        foreach (var row in await QueryDatabaseAsync(source, credential, MainDatabaseName, localStart, localEnd, true, cancellationToken))
            rows.Add(row.SourceRecordId, row);

        // Historical databases are best effort. Discovery and any individual historical failure are omitted.
        foreach (var database in await GetHistoricalDatabasesAsync(source, credential, cancellationToken))
            foreach (var row in await QueryDatabaseAsync(source, credential, database, localStart, localEnd, false, cancellationToken))
                rows.TryAdd(row.SourceRecordId, row); // The main Hadi record has priority over historical copies.

        return rows.Values.ToArray();
    }

    private async Task<IReadOnlyCollection<ExtractedSaleRow>> QueryDatabaseAsync(DataSource source, DataSourceCredential credential, string database, DateTime start, DateTime end, bool isRequired, CancellationToken token)
    {
        try
        {
            await using var connection = new SqlConnection(BuildConnectionString(source, credential, database));
            await connection.OpenAsync(token);
            await using var command = new SqlCommand("dbo.RecuperarVentasEnergigas", connection)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = CommandTimeoutSeconds
            };
            command.Parameters.Add(new SqlParameter("@FechaInicial", SqlDbType.DateTime) { Value = start });
            command.Parameters.Add(new SqlParameter("@FechaFinal", SqlDbType.DateTime) { Value = end });

            var rows = new List<ExtractedSaleRow>();
            var localKeys = new HashSet<string>(StringComparer.Ordinal);
            await using var reader = await command.ExecuteReaderAsync(token);
            while (await reader.ReadAsync(token))
            {
                var values = ReadValues(reader);
                values["source_database"] = database;
                var sourceRecordId = BuildSourceRecordId(values);
                if (!localKeys.Add(sourceRecordId))
                    throw new InvalidOperationException($"Gasolution devolvió una clave condicional duplicada '{sourceRecordId}' en la base '{database}'.");
                rows.Add(new ExtractedSaleRow(sourceRecordId, values, null));
            }
            return rows;
        }
        catch (Exception) when (!isRequired && !token.IsCancellationRequested)
        {
            return Array.Empty<ExtractedSaleRow>();
        }
    }

    private async Task<IReadOnlyList<string>> GetHistoricalDatabasesAsync(DataSource source, DataSourceCredential credential, CancellationToken token)
    {
        const string sql = @"
            SELECT TOP (2) name
            FROM sys.databases
            WHERE name LIKE 'HadiHistorico%'
              AND state_desc = 'ONLINE'
              AND HAS_DBACCESS(name) = 1
            ORDER BY create_date DESC;";
        try
        {
            await using var connection = new SqlConnection(BuildConnectionString(source, credential, MainDatabaseName));
            await connection.OpenAsync(token);
            await using var command = new SqlCommand(sql, connection) { CommandTimeout = CommandTimeoutSeconds };
            await using var reader = await command.ExecuteReaderAsync(token);
            var databases = new List<string>();
            while (await reader.ReadAsync(token))
            {
                var database = reader.GetString(0);
                if (!database.Equals(MainDatabaseName, StringComparison.OrdinalIgnoreCase)) databases.Add(database);
            }
            return databases;
        }
        catch (Exception) when (!token.IsCancellationRequested)
        {
            return Array.Empty<string>();
        }
    }

    private static Dictionary<string, object?> ReadValues(SqlDataReader reader)
    {
        var values = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        for (var index = 0; index < reader.FieldCount; index++) values[reader.GetName(index)] = reader.IsDBNull(index) ? null : reader.GetValue(index);
        return values;
    }

    // Validated Argentina rule: DOC|Recibo|Serie|Numero, otherwise VF|Recibo|IdVentaFacturacion.
    private static string BuildSourceRecordId(IReadOnlyDictionary<string, object?> row)
    {
        var receipt = Required(row, "Recibo");
        var serie = Optional(row, "Serie");
        var numero = Optional(row, "Numero");
        if (serie is not null || numero is not null)
        {
            if (serie is null || numero is null) throw new InvalidOperationException("Gasolution devolvió Serie o Numero sin su componente complementario.");
            return $"DOC|{receipt}|{serie}|{numero}";
        }
        return $"VF|{receipt}|{Required(row, "IdVentaFacturacion")}";
    }

    private static string Required(IReadOnlyDictionary<string, object?> row, string column) => Optional(row, column) ?? throw new InvalidOperationException($"Gasolution devolvió NULL {column}; no puede formar la clave estable.");
    private static string? Optional(IReadOnlyDictionary<string, object?> row, string column) => row.TryGetValue(column, out var value) && value is not null ? Convert.ToString(value, CultureInfo.InvariantCulture)?.Trim() : null;
    private static string BuildConnectionString(DataSource source, DataSourceCredential credential, string database)
    {
        var dataSource = source.Server.Contains('\\') || source.Server.Contains(',') ? source.Server : $"{source.Server},{source.Port}";
        return new SqlConnectionStringBuilder { DataSource = dataSource, InitialCatalog = database, UserID = credential.UserName, Password = credential.Password, ConnectTimeout = 30, TrustServerCertificate = true, Encrypt = false, ApplicationName = "StationSales-Gasolution" }.ConnectionString;
    }
    private static DateTime ToLimaDate(DateTime utc) => TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc), TimeZoneInfo.FindSystemTimeZoneById("America/Lima")).Date;
}
