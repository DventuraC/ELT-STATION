using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StationSales.Application;
using StationSales.Infrastructure;

namespace StationSales.Worker;

public interface ISourceSystemExtractionWorker { Task<int> ExecuteAsync(CancellationToken cancellationToken); }

/// <summary>Runs one isolated extraction scope per station; EF DbContexts are never shared between parallel tasks.</summary>
public abstract class SourceSystemExtractionWorker : ISourceSystemExtractionWorker
{
    private const string ExtractionPipelineCode = "EXTRACT_STATION_SALES";
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger _logger;
    private readonly long _sourceSystemId; private readonly Func<SourceSystemWorkerOptions> _options;
    protected SourceSystemExtractionWorker(IServiceScopeFactory scopeFactory, ILogger logger, long sourceSystemId, Func<SourceSystemWorkerOptions> options) => (_scopeFactory, _logger, _sourceSystemId, _options) = (scopeFactory, logger, sourceSystemId, options);

    public async Task<int> ExecuteAsync(CancellationToken cancellationToken)
    {
        (long PipelineDataSourceId, string SourceCode, int IntervalMinutes)[] configurations;
        using (var scope = _scopeFactory.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<StationSalesDbContext>();
            var activeConfigurations = await db.PipelineDataSources.AsNoTracking()
                .Where(x => x.IsActive && x.Pipeline.IsActive && x.Pipeline.Code == ExtractionPipelineCode && x.DataSource.IsActive && x.DataSource.SourceSystemId == _sourceSystemId)
                .Select(x => new { x.Id, x.DataSource.Code, x.IntervalMinutes }).ToArrayAsync(cancellationToken);
            var ids = activeConfigurations.Select(x => x.Id).ToArray();
            var latestRuns = await db.ExtractionRuns.AsNoTracking().Where(x => ids.Contains(x.PipelineDataSourceId))
                .GroupBy(x => x.PipelineDataSourceId).Select(x => new { PipelineDataSourceId = x.Key, StartedAtUtc = x.Max(r => r.StartedAtUtc) }).ToDictionaryAsync(x => x.PipelineDataSourceId, x => x.StartedAtUtc, cancellationToken);
            var now = DateTime.UtcNow;
            configurations = activeConfigurations.Where(x => !latestRuns.TryGetValue(x.Id, out var lastRun) || lastRun <= now.AddMinutes(-Math.Max(1, x.IntervalMinutes)))
                .Select(x => (x.Id, x.Code, x.IntervalMinutes)).ToArray();
        }

        var failures = 0;
        await Parallel.ForEachAsync(configurations, new ParallelOptions { MaxDegreeOfParallelism = Math.Max(1, _options().MaxDegreeOfParallelism), CancellationToken = cancellationToken }, async (configuration, token) =>
        {
            using var scope = _scopeFactory.CreateScope();
            try
            {
                var result = await scope.ServiceProvider.GetRequiredService<ExtractStationSalesUseCase>().ExecuteAsync(configuration.SourceCode, token);
                if (result > 0) Interlocked.Add(ref failures, result);
            }
            catch (Exception ex)
            {
                Interlocked.Increment(ref failures);
                _logger.LogError(ex, "Extraction worker failed for source {SourceCode}", configuration.SourceCode);
            }
        });
        return failures;
    }
}

public sealed class OpenCombExtractionWorker : SourceSystemExtractionWorker
{
    public OpenCombExtractionWorker(IServiceScopeFactory scopeFactory, ILogger<OpenCombExtractionWorker> logger, IOptions<WorkerScheduleOptions> options) : base(scopeFactory, logger, 1, () => options.Value.OpenComb) { }
}

public sealed class GasolutionExtractionWorker : SourceSystemExtractionWorker
{
    public GasolutionExtractionWorker(IServiceScopeFactory scopeFactory, ILogger<GasolutionExtractionWorker> logger, IOptions<WorkerScheduleOptions> options) : base(scopeFactory, logger, 2, () => options.Value.Gasolution) { }
}
