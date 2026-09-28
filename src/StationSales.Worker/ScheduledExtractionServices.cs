using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace StationSales.Worker;

public sealed class WorkerScheduleOptions
{
    public SourceSystemWorkerOptions OpenComb { get; set; } = new();
    public SourceSystemWorkerOptions Gasolution { get; set; } = new();
}
public sealed class SourceSystemWorkerOptions
{
    public bool Enabled { get; set; }
    public bool RunOnStartup { get; set; } = true;
    public int IntervalMinutes { get; set; } = 60;
    public int MaxDegreeOfParallelism { get; set; } = 4;
}

public abstract class ScheduledExtractionService : BackgroundService
{
    private readonly ISourceSystemExtractionWorker _worker;
    private readonly SourceSystemWorkerOptions _options;
    private readonly ILogger _logger;
    protected ScheduledExtractionService(ISourceSystemExtractionWorker worker, SourceSystemWorkerOptions options, ILogger logger) => (_worker, _options, _logger) = (worker, options, logger);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled) { _logger.LogInformation("{Worker} is disabled by configuration.", GetType().Name); return; }
        if (!_options.RunOnStartup) await DelayAsync(stoppingToken);
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var failures = await _worker.ExecuteAsync(stoppingToken);
                if (failures > 0) _logger.LogWarning("{Worker} finished with {Failures} failed source(s).", GetType().Name, failures);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
            catch (Exception ex) { _logger.LogError(ex, "{Worker} failed outside an individual source scope.", GetType().Name); }
            await DelayAsync(stoppingToken);
        }
    }
    private Task DelayAsync(CancellationToken token) => Task.Delay(TimeSpan.FromMinutes(Math.Max(1, _options.IntervalMinutes)), token);
}

public sealed class OpenCombScheduledExtractionService : ScheduledExtractionService
{
    public OpenCombScheduledExtractionService(OpenCombExtractionWorker worker, IOptions<WorkerScheduleOptions> options, ILogger<OpenCombScheduledExtractionService> logger) : base(worker, options.Value.OpenComb, logger) { }
}
public sealed class GasolutionScheduledExtractionService : ScheduledExtractionService
{
    public GasolutionScheduledExtractionService(GasolutionExtractionWorker worker, IOptions<WorkerScheduleOptions> options, ILogger<GasolutionScheduledExtractionService> logger) : base(worker, options.Value.Gasolution, logger) { }
}
