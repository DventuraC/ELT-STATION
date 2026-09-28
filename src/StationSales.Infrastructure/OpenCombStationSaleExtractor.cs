using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Npgsql;
using StationSales.Application;
using StationSales.Domain;

namespace StationSales.Infrastructure;

/// <summary>Reads the fixed OpenComb sales contract. The monthly table is derived only from dates, never from user input.</summary>
public sealed class OpenCombStationSaleExtractor : IStationSaleExtractor
{
    private const int CommandTimeoutSeconds = 150;
    private readonly IDataSourceSecretResolver _secrets;
    public OpenCombStationSaleExtractor(IDataSourceSecretResolver secrets) => _secrets = secrets;
    public SourceProvider Provider => SourceProvider.OpenComb;
    public bool ProvidesCompleteDocumentSnapshot => true;

    public async Task<IReadOnlyCollection<ExtractedSaleRow>> ExtractAsync(DataSource source, ExtractionWindow window, CancellationToken cancellationToken)
    {
        var credential = _secrets.Resolve(source.SecretReference);
        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = source.Server, Port = source.Port, Database = source.DatabaseName,
            Username = credential.UserName, Password = credential.Password,
            Timeout = 30, CommandTimeout = CommandTimeoutSeconds, Pooling = true
        };
        await using var connection = new NpgsqlConnection(builder.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        // OpenComb stores timestamps without zone. Treat the ELT window as Lima-local business dates.
        var localStart = ToLimaDate(window.FromUtc);
        var localEnd = ToLimaDate(window.ToUtc);
        var currentTable = MonthlyTable(localEnd);
        await ValidateCurrentTableAccessAsync(connection, currentTable, cancellationToken);

        var availableTables = await GetAvailableTablesAsync(connection, localStart, localEnd, cancellationToken);
        var candidates = new List<(string StableDetailKey, Dictionary<string, object?> Values, DateTime? SourceUpdatedAtUtc)>();
        foreach (var table in availableTables)
        {
            await using var command = CreateSalesCommand(connection, table, localStart, localEnd.AddDays(1));
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                var values = ReadContractRow(reader);
                values["source_table"] = table;
                candidates.Add((BuildStableDetailKey(values), values, GetDateTime(values, "fecha_replicacion")));
            }
        }

        var rows = new List<ExtractedSaleRow>(candidates.Count);
        foreach (var group in candidates.GroupBy(x => x.StableDetailKey, StringComparer.Ordinal))
        {
            if (group.Count() == 1)
            {
                var candidate = group.Single();
                rows.Add(new ExtractedSaleRow(candidate.StableDetailKey, candidate.Values, candidate.SourceUpdatedAtUtc));
                continue;
            }

            // The document and product key stays stable. A content hash distinguishes repeated product lines.
            var fingerprints = new HashSet<string>(StringComparer.Ordinal);
            foreach (var candidate in group)
            {
                var fingerprint = BuildDetailContentHash(candidate.Values);
                if (!fingerprints.Add(fingerprint))
                    throw new InvalidOperationException($"OpenComb returned indistinguishable duplicate details for source '{source.Code}', stable key '{group.Key}'.");
                rows.Add(new ExtractedSaleRow($"{candidate.StableDetailKey}|{fingerprint}", candidate.Values, candidate.SourceUpdatedAtUtc));
            }
        }
        return rows;
    }

    private static async Task ValidateCurrentTableAccessAsync(NpgsqlConnection connection, string table, CancellationToken token)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = $"SELECT 1 FROM public.{QuoteIdentifier(table)} LIMIT 1;";
        try { await command.ExecuteScalarAsync(token); }
        catch (PostgresException ex) when (ex.SqlState == "42P01") { throw new InvalidOperationException($"La tabla OpenComb del mes actual '{table}' no existe.", ex); }
        catch (PostgresException ex) when (ex.SqlState == "42501") { throw new InvalidOperationException($"No hay permisos para consultar la tabla OpenComb '{table}'.", ex); }
    }

    private static async Task<IReadOnlyList<string>> GetAvailableTablesAsync(NpgsqlConnection connection, DateTime start, DateTime end, CancellationToken token)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT tablename FROM pg_tables
            WHERE schemaname = 'public' AND tablename >= @start AND tablename <= @end
              AND tablename ~ '^pos_trans[0-9]{6}$'
            ORDER BY tablename;";
        command.Parameters.AddWithValue("start", MonthlyTable(start));
        command.Parameters.AddWithValue("end", MonthlyTable(end));
        var tables = new List<string>();
        await using var reader = await command.ExecuteReaderAsync(token);
        while (await reader.ReadAsync(token)) tables.Add(reader.GetString(0));
        return tables;
    }

    private static NpgsqlCommand CreateSalesCommand(NpgsqlConnection connection, string table, DateTime from, DateTime until)
    {
        var command = connection.CreateCommand(); command.CommandTimeout = CommandTimeoutSeconds;
        command.CommandText = $@"
            SELECT v2.art_descripcion,
                   v1.razsocial AS razon_social,
                   v0.ruc, v0.tm, v0.caja, v0.td, v0.dia, v0.turno, v0.grupo, v0.codigo,
                   v0.cantidad, v0.precio, v0.igv, v0.importe, v0.cajero, v0.fecha, v0.tipo,
                   v0.pump, v0.fpago, v0.at, v0.text1, v0.tarjeta, v0.km, v0.rendi_gln,
                   v0.rendi_acu, v0.soles_km, v0.placa, v0.nombre, v0.usr, v0.es,
                   v0.flg_replicacion, v0.fecha_replicacion, v0.trans, v0.ebidata
            FROM public.{QuoteIdentifier(table)} v0
            LEFT JOIN int_articulos v2 ON v0.codigo = v2.art_codigo
            LEFT JOIN ruc v1 ON v0.ruc = v1.ruc
            WHERE v0.fecha >= @from AND v0.fecha < @until;";
        command.Parameters.AddWithValue("from", DateTime.SpecifyKind(from, DateTimeKind.Unspecified));
        command.Parameters.AddWithValue("until", DateTime.SpecifyKind(until, DateTimeKind.Unspecified));
        return command;
    }

    private static Dictionary<string, object?> ReadContractRow(DbDataReader reader)
    {
        var result = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        for (var i = 0; i < reader.FieldCount; i++) result[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
        return result;
    }

    // The stable business detail key identifies the document/product. Repeated product lines use a content hash suffix.
    private static string BuildStableDetailKey(IReadOnlyDictionary<string, object?> row) => string.Join("|", new[] { "es", "td", "grupo", "trans", "codigo" }.Select(x => RequiredKeyValue(row, x)));
    private static string BuildDetailContentHash(IReadOnlyDictionary<string, object?> row)
    {
        var canonical = string.Join("\u001F", row.OrderBy(x => x.Key, StringComparer.Ordinal).Select(x => $"{x.Key}\u001E{CanonicalValue(x.Value)}"));
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonical)));
    }
    private static string CanonicalValue(object? value) => value switch
    {
        null => "<NULL>",
        DateTime date => date.ToString("O", CultureInfo.InvariantCulture),
        IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
        _ => value.ToString() ?? string.Empty
    };
    private static string RequiredKeyValue(IReadOnlyDictionary<string, object?> row, string field) => row.TryGetValue(field, out var value) && value is not null ? Convert.ToString(value, CultureInfo.InvariantCulture)!.Trim() : throw new InvalidOperationException($"OpenComb row has NULL {field}; it cannot form the stable detail key.");
    private static DateTime? GetDateTime(IReadOnlyDictionary<string, object?> row, string field) => row.TryGetValue(field, out var value) && value is DateTime date ? date : null;
    private static string MonthlyTable(DateTime value) => $"pos_trans{value:yyyyMM}";
    private static string QuoteIdentifier(string identifier) => $"\"{identifier.Replace("\"", "\"\"")}\"";
    private static DateTime ToLimaDate(DateTime utc) => TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc), TimeZoneInfo.FindSystemTimeZoneById("America/Lima")).Date;
}

public sealed class OpenCombHashContract
{
    private readonly IHashService _hashes;
    public OpenCombHashContract(IHashService hashes) => _hashes = hashes;
    public byte[] DocumentKeyHash(long dataSourceId, string es, string td, string grupo, decimal trans) => _hashes.Compute("OPENCOMB", dataSourceId, es, td, grupo, trans);
    public byte[] DetailKeyHash(byte[] documentKeyHash, string codigo) => _hashes.Compute(documentKeyHash, codigo);
    public byte[] DetailHash(byte[] detailKeyHash, decimal cantidad, decimal precio, decimal igv, decimal importe) => _hashes.Compute(detailKeyHash, cantidad, precio, igv, importe);
}
