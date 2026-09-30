using StationSales.Domain;

namespace StationSales.Application;

public sealed class SystemClock : IClock { public DateTime UtcNow => DateTime.UtcNow; }
public sealed class IncrementalWindowCalculator
{
    public ExtractionWindow Calculate(PipelineDataSource configuration, DateTime nowUtc)
    {
        if (configuration.RecoveryDays < 0) throw new ArgumentOutOfRangeException(nameof(configuration.RecoveryDays));
        var checkpoint = configuration.LastSuccessfulDate ?? configuration.StartDate;
        var recoveredFrom = checkpoint.AddDays(-configuration.RecoveryDays);
        var fromUtc = recoveredFrom < configuration.StartDate ? configuration.StartDate : recoveredFrom;
        return new ExtractionWindow(fromUtc, nowUtc);
    }
}
public sealed class RawWriteCoordinator : IRawWriteCoordinator, IDisposable
{
    private readonly SemaphoreSlim _openComb = new(1, 1);
    private readonly SemaphoreSlim _gasolution = new(1, 1);

    public async Task<T> ExecuteAsync<T>(SourceProvider provider, Func<CancellationToken, Task<T>> operation, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(operation);
        var gate = provider switch
        {
            SourceProvider.OpenComb => _openComb,
            SourceProvider.Gasolution => _gasolution,
            _ => throw new ArgumentOutOfRangeException(nameof(provider), provider, "Unsupported source provider.")
        };

        await gate.WaitAsync(cancellationToken);
        try { return await operation(cancellationToken); }
        finally { gate.Release(); }
    }

    public void Dispose()
    {
        _openComb.Dispose();
        _gasolution.Dispose();
    }
}
public sealed class ExtractStationSalesUseCase
{
    private readonly IExtractionRepository _repository; private readonly IExtractorResolver _extractors; private readonly IRawSaleWriter _writer; private readonly IRawWriteCoordinator _writeCoordinator; private readonly IClock _clock; private readonly IncrementalWindowCalculator _windows;
    public ExtractStationSalesUseCase(IExtractionRepository repository, IExtractorResolver extractors, IRawSaleWriter writer, IRawWriteCoordinator writeCoordinator, IClock clock, IncrementalWindowCalculator windows) => (_repository, _extractors, _writer, _writeCoordinator, _clock, _windows) = (repository, extractors, writer, writeCoordinator, clock, windows);
    public async Task<int> ExecuteAsync(string? sourceCode, CancellationToken cancellationToken)
    {
        var failures = 0;
        foreach (var config in await _repository.GetActiveAsync(sourceCode, cancellationToken))
        {
            var now = _clock.UtcNow; var window = _windows.Calculate(config, now); var run = await _repository.StartAsync(config, window, cancellationToken);
            try
            {
                var rows = await _extractors.Resolve(config.DataSource.Provider).ExtractAsync(config.DataSource, window, cancellationToken);
                if (rows.Count == 0) { await _repository.SucceedWithNoDataAsync(run, cancellationToken); continue; }
                var written = await _writeCoordinator.ExecuteAsync(config.DataSource.Provider, token => _writer.WriteAsync(run, config.DataSource, rows, token), cancellationToken);
                await _repository.SucceedAsync(run, config, rows.Count, written, now, cancellationToken);
            }
            catch (Exception ex) { failures++; await _repository.FailAsync(run, ex, cancellationToken); }
        }
        return failures;
    }
    public async Task<int> ExecuteBackfillAsync(string sourceCode, ExtractionWindow window, CancellationToken cancellationToken)
    {
        var failures = 0;
        foreach (var config in await _repository.GetActiveAsync(sourceCode, cancellationToken))
        {
            var run = await _repository.StartAsync(config, window, cancellationToken);
            try
            {
                var rows = await _extractors.Resolve(config.DataSource.Provider).ExtractAsync(config.DataSource, window, cancellationToken);
                if (rows.Count == 0) { await _repository.SucceedWithNoDataAsync(run, cancellationToken); continue; }
                var written = await _writeCoordinator.ExecuteAsync(config.DataSource.Provider, token => _writer.WriteAsync(run, config.DataSource, rows, token), cancellationToken);
                await _repository.SucceedWithoutCheckpointAsync(run, rows.Count, written, cancellationToken);
            }
            catch (Exception ex) { failures++; await _repository.FailAsync(run, ex, cancellationToken); }
        }
        return failures;
    }
}
public sealed class CoreSynchronizer
{
    private readonly ICoreRepository _repository; private readonly IClock _clock;
    public CoreSynchronizer(ICoreRepository repository, IClock clock) => (_repository, _clock) = (repository, clock);
    public async Task SynchronizeAsync(long runId, IReadOnlyCollection<NormalizedSale> rows, CancellationToken token)
    {
        foreach (var group in rows.GroupBy(x => Convert.ToBase64String(x.DocumentKeyHash)))
        {
            var first = group.First(); var document = await _repository.FindDocumentAsync(first.DocumentKeyHash, token);
            if (document is null) { document = new SaleDocument(first.DocumentKeyHash, first.DocumentHash, first.Provider, first.StationCode, runId, _clock.UtcNow); await _repository.AddDocumentAsync(document, token); }
            else document.Synchronize(first.DocumentHash, runId, _clock.UtcNow);
            foreach (var row in group) { var detail = document.Details.SingleOrDefault(x => x.DetailKeyHash.SequenceEqual(row.DetailKeyHash)); if (detail is null) document.Details.Add(new SaleDocumentDetail(row.DetailKeyHash, row.DetailHash, runId, _clock.UtcNow)); else detail.Synchronize(row.DetailHash, runId, _clock.UtcNow); }
            if (group.All(x => x.CompleteDocumentSnapshot)) foreach (var old in document.Details.Where(x => x.IsActive && x.LastSeenExtractionRunId != runId)) old.SoftDelete(_clock.UtcNow);
        }
        await _repository.SaveChangesAsync(token);
    }
}
