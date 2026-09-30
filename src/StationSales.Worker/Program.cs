using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StationSales.Application;
using StationSales.Infrastructure;
using StationSales.Worker;

var host = Host.CreateDefaultBuilder(args)
    .UseContentRoot(AppContext.BaseDirectory)
    .ConfigureLogging(logging =>
    {
        logging.ClearProviders();
        logging.AddConsole();
    })
    .ConfigureServices((context, services) =>
{
    var connection = context.Configuration.GetConnectionString("CentralDatabase") ?? throw new InvalidOperationException("ConnectionStrings:CentralDatabase is required.");
    services.AddDbContext<StationSalesDbContext>(x => x.UseSqlServer(connection));
    services.AddSingleton<IClock, SystemClock>(); services.AddSingleton<IHashService, DeterministicSha256HashService>();
    services.AddSingleton<IDataSourceSecretResolver, ConfigurationDataSourceSecretResolver>();
    services.AddSingleton<IRawWriteCoordinator, RawWriteCoordinator>();
    services.AddScoped<IExtractionRepository, EfExtractionRepository>(); services.AddScoped<IRawSaleWriter, EfRawSaleWriter>(); services.AddScoped<ICoreRepository, EfCoreRepository>(); services.AddScoped<IConfigurationSeeder, ConfigurationSeeder>();
    services.AddSingleton<IStationSaleExtractor, OpenCombStationSaleExtractor>(); services.AddSingleton<IStationSaleExtractor, GasolutionStationSaleExtractor>(); services.AddSingleton<IExtractorResolver, ExtractorResolver>();
    services.AddScoped<IncrementalWindowCalculator>(); services.AddScoped<ExtractStationSalesUseCase>(); services.AddScoped<CoreSynchronizer>();
    services.Configure<WorkerScheduleOptions>(context.Configuration.GetSection("Workers"));
    services.AddSingleton<OpenCombExtractionWorker>(); services.AddSingleton<GasolutionExtractionWorker>();
    services.AddHostedService<OpenCombScheduledExtractionService>(); services.AddHostedService<GasolutionScheduledExtractionService>();
    services.AddSingleton<IMartPublisher, SqlServerMartPublisher>(); services.AddSingleton<IMartPublisher, PostgreSqlMartPublisher>();
}).Build();

var command = args.FirstOrDefault()?.ToLowerInvariant();
if (command is null) { await host.RunAsync(); return 0; }
if (command is not ("seed" or "extract" or "extract-opencomb" or "extract-gasolution" or "backfill-opencomb" or "backfill-gasolution" or "transform" or "publish" or "run-all")) { Console.Error.WriteLine("Usage: StationSales.Worker <seed|extract|extract-opencomb|extract-gasolution|backfill-opencomb|backfill-gasolution|transform|publish|run-all> [--source CODE] [--from yyyy-MM-dd --to yyyy-MM-dd] [--destination sqlserver|postgres]"); return 2; }
using var scope = host.Services.CreateScope();
if (command is "seed")
{
    await scope.ServiceProvider.GetRequiredService<IConfigurationSeeder>().SeedAsync(CancellationToken.None);
    Console.WriteLine("Configuration seed completed.");
}
else if (command is "extract-opencomb")
{
    if (await scope.ServiceProvider.GetRequiredService<OpenCombExtractionWorker>().ExecuteAsync(CancellationToken.None) > 0) return 1;
}
else if (command is "extract-gasolution")
{
    if (await scope.ServiceProvider.GetRequiredService<GasolutionExtractionWorker>().ExecuteAsync(CancellationToken.None) > 0) return 1;
}
else if (command is "backfill-opencomb" or "backfill-gasolution")
{
    static string? Option(string[] values, string name) => values.Skip(1).SkipWhile(x => x != name).Skip(1).FirstOrDefault();
    static DateTime LimaUtc(string value)
    {
        var date = DateTime.ParseExact(value, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
        return TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(date, DateTimeKind.Unspecified), TimeZoneInfo.FindSystemTimeZoneById("America/Lima"));
    }
    var fromText = Option(args, "--from") ?? throw new ArgumentException("--from yyyy-MM-dd is required for a backfill.");
    var toText = Option(args, "--to") ?? throw new ArgumentException("--to yyyy-MM-dd is required for a backfill.");
    var requestedSource = Option(args, "--source");
    var window = new ExtractionWindow(LimaUtc(fromText), LimaUtc(toText));
    if (window.ToUtc < window.FromUtc) throw new ArgumentException("--to cannot be before --from.");
    var sourceSystemId = command == "backfill-opencomb" ? 1L : 2L;
    var providerName = command == "backfill-opencomb" ? "OpenComb" : "Gasolution";
    var sources = await scope.ServiceProvider.GetRequiredService<StationSalesDbContext>().PipelineDataSources.AsNoTracking()
        .Where(x => x.IsActive && x.Pipeline.IsActive && x.Pipeline.Code == "EXTRACT_STATION_SALES" && x.DataSource.IsActive && x.DataSource.SourceSystemId == sourceSystemId && (requestedSource == null || x.DataSource.Code == requestedSource))
        .Select(x => x.DataSource.Code).ToArrayAsync();
    if (requestedSource is not null && sources.Length == 0)
    {
        Console.Error.WriteLine($"Active {providerName} source '{requestedSource}' was not found.");
        return 2;
    }
    var failures = 0;
    await Parallel.ForEachAsync(sources, new ParallelOptions { MaxDegreeOfParallelism = 4 }, async (source, token) =>
    {
        using var sourceScope = host.Services.CreateScope();
        try { var result = await sourceScope.ServiceProvider.GetRequiredService<ExtractStationSalesUseCase>().ExecuteBackfillAsync(source, window, token); if (result > 0) Interlocked.Add(ref failures, result); }
        catch { Interlocked.Increment(ref failures); }
    });
    var sourceLabel = requestedSource is null ? "all active sources" : requestedSource;
    Console.WriteLine($"{providerName} backfill {fromText} to {toText} ({sourceLabel}): {sources.Length - failures}/{sources.Length} sources completed.");
    if (failures > 0) return 1;
}
else if (command is "extract" or "run-all")
{
    var source = args.Skip(1).SkipWhile(x => x != "--source").Skip(1).FirstOrDefault();
    var failures = source is null
        ? await scope.ServiceProvider.GetRequiredService<OpenCombExtractionWorker>().ExecuteAsync(CancellationToken.None) + await scope.ServiceProvider.GetRequiredService<GasolutionExtractionWorker>().ExecuteAsync(CancellationToken.None)
        : await scope.ServiceProvider.GetRequiredService<ExtractStationSalesUseCase>().ExecuteAsync(source, CancellationToken.None);
    if (failures > 0) return 1;
}
if (command is "transform" or "run-all") Console.WriteLine("Transform is ready for provider-specific normalization contracts; no source fields have been assumed.");
if (command is "publish" or "run-all")
{
    var destination = args.Skip(1).SkipWhile(x => x != "--destination").Skip(1).FirstOrDefault();
    foreach (var publisher in scope.ServiceProvider.GetServices<IMartPublisher>().Where(x => destination is null || x.Destination.ToString().Equals(destination, StringComparison.OrdinalIgnoreCase))) await publisher.PublishAsync(CancellationToken.None);
}
return 0;
