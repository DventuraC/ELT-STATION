using Microsoft.EntityFrameworkCore;
using StationSales.Application;
using StationSales.Domain;

namespace StationSales.Infrastructure;

public interface IConfigurationSeeder { Task SeedAsync(CancellationToken cancellationToken); }

/// <summary>Idempotent operational configuration seed. Credentials are deliberately excluded.</summary>
public sealed class ConfigurationSeeder : IConfigurationSeeder
{
    private static readonly DateTime DefaultStartDate = new(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc);
    private readonly StationSalesDbContext _db; private readonly IClock _clock;
    public ConfigurationSeeder(StationSalesDbContext db, IClock clock) => (_db, _clock) = (db, clock);

    public async Task SeedAsync(CancellationToken token)
    {
        var now = _clock.UtcNow;
        var pipeline = await _db.Pipelines.SingleOrDefaultAsync(x => x.Code == "EXTRACT_STATION_SALES", token);
        if (pipeline is null) { pipeline = new Pipeline { Code = "EXTRACT_STATION_SALES", Name = "Extraccion de ventas de estaciones", Description = "Carga RAW para OpenComb y Gasolution", PipelineType = PipelineType.Extract, IsActive = true }; _db.Pipelines.Add(pipeline); await _db.SaveChangesAsync(token); }
        var stations = await _db.Stations.ToDictionaryAsync(x => x.Code, token);
        var systems = await _db.SourceSystems.ToDictionaryAsync(x => x.Code, token);
        var existing = await _db.DataSources.ToDictionaryAsync(x => x.Code, token);
        foreach (var item in Items)
        {
            if (!stations.TryGetValue(item.StationCode, out var station)) throw new InvalidOperationException($"Station seed '{item.StationCode}' is missing.");
            if (!systems.TryGetValue(item.SourceSystemCode, out var system)) throw new InvalidOperationException($"Source system seed '{item.SourceSystemCode}' is missing.");
            if (!existing.TryGetValue(item.Code, out var source)) { source = new DataSource { Code = item.Code, CreatedAtUtc = now }; _db.DataSources.Add(source); existing.Add(item.Code, source); }
            source.StationId = station.Id; source.SourceSystemId = system.Id; source.Name = station.Name; source.StationCode = station.Code; source.Provider = item.Provider; source.DatabaseType = item.DatabaseType; source.Server = item.Server; source.Port = item.Port; source.DatabaseName = item.DatabaseName; source.SecretReference = item.SecretReference; source.Environment = "PROD"; source.IsActive = item.IsActive; source.UpdatedAtUtc = now;
        }
        await _db.SaveChangesAsync(token);
        var codes = Items.Select(x => x.Code).ToArray();
        var sourceIds = await _db.DataSources.Where(x => codes.Contains(x.Code)).ToDictionaryAsync(x => x.Code, x => x.Id, token);
        var links = await _db.PipelineDataSources.Where(x => x.PipelineId == pipeline.Id).ToDictionaryAsync(x => x.DataSourceId, token);
        foreach (var item in Items)
        {
            var sourceId = sourceIds[item.Code];
            if (!links.TryGetValue(sourceId, out var link)) { link = new PipelineDataSource { PipelineId = pipeline.Id, DataSourceId = sourceId, CreatedAtUtc = now }; _db.PipelineDataSources.Add(link); }
            link.IsActive = item.IsActive; link.StartDate = item.StartDate; link.RecoveryDays = 5; link.IntervalMinutes = 180; link.UpdatedAtUtc = now;
        }
        await _db.SaveChangesAsync(token);
    }

    private sealed record Item(string Code, string SourceSystemCode, string StationCode, SourceProvider Provider, DatabaseType DatabaseType, string Server, int Port, string DatabaseName, string SecretReference, bool IsActive, DateTime StartDate);
    private static Item O(string station, string server, string secret, bool active = true) => new($"OPENCOMB_{station}", "OPENCOMB", station, SourceProvider.OpenComb, DatabaseType.PostgreSql, server, 5432, "integrado", secret, active, DefaultStartDate);
    private static Item G(string station, string server, bool active = true, DateTime? start = null) => new($"GASOLUTION_{station}", "GASOLUTION", station, SourceProvider.Gasolution, DatabaseType.SqlServer, server, 1433, "Hadi", "energigas", active, start ?? DefaultStartDate);
    private static readonly Item[] Items =
    {
        O("EDS_ICA", "10.0.18.1", "opencomb_eds_ica"), O("EDS_VENEZUELA", "10.0.23.1", "opencomb_eds_venezuela"), O("EDS_LA_MOLINA", "10.0.26.1", "opencomb_eds_victoria"), O("EDS_VICTORIA", "10.0.27.1", "opencomb_eds_victoria_2"), O("EDS_VICTORIA_2", "10.0.30.1", "opencomb_eds_barranco"), O("EDS_BARRANCO", "10.0.20.1", "opencomb_eds_marina", false), O("EDS_MARINA", "10.0.24.1", "opencomb_eds_nuevo_chimbote"), O("EDS_NUEVO_CHIMBOTE", "10.0.19.1", "opencomb_eds_argentina"), O("EDS_ARGENTINA", "10.0.25.1", "opencomb_eds_callao"), O("EDS_CALLAO", "10.0.90.200", "opencomb_eds_chincha"), O("EDS_CHINCHA", "10.0.33.201", "opencomb_eds_puente_piedra"), O("EDS_PUENTE_PIEDRA", "10.0.35.1", "opencomb_eds_lurin"), O("EDS_LURIN", "10.0.50.200", "opencomb_eds_ate"), O("EDS_ATE", "10.0.37.1", "opencomb_eds_chimbote_2", false), O("EDS_CHIMBOTE_2", "10.1.50.201", "opencomb_eds_trujillo"), O("EDS_TRUJILLO", "10.1.40.150", "opencomb_eds_tomas_valle"), O("EDS_TOMAS_VALLE", "10.0.60.201", "opencomb_eds_campoy"), O("EDS_VENEZUELA_2", "10.0.38.200", "opencomb_eds_paita"),
        G("EDS_ICA", "10.0.18.30"), G("EDS_VENEZUELA", "10.0.23.30"), G("EDS_LA_MOLINA", "10.0.26.62"), G("EDS_VICTORIA_2", "10.0.30.30"), G("EDS_MARINA", "10.0.24.30"), G("EDS_NUEVO_CHIMBOTE", "10.0.19.30"), G("EDS_ARGENTINA", "10.0.25.30"), G("EDS_CALLAO", "10.0.90.111\\SQLEXPRESS"), G("EDS_CHINCHA", "10.0.33.30\\SQLEXPRESS"), G("EDS_LURIN", "10.0.50.120"), G("EDS_ATE", "10.0.37.30", false), G("EDS_TOMAS_VALLE", "10.0.60.74"), G("EDS_CAMPOY", "10.0.36.30\\SQLEXPRESS"), G("EDS_VENEZUELA_2", "10.0.38.30"), G("EDS_PAITA", "10.0.39.30\\SQLEXPRESS", true, new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc))
    };
}
