using StationSales.Domain;

namespace StationSales.Application;

public record ExtractionWindow(DateTime FromUtc, DateTime ToUtc);
public record ExtractedSaleRow(string SourceRecordId, IReadOnlyDictionary<string, object?> Values, DateTime? SourceUpdatedAtUtc);
public record NormalizedSale(string StationCode, SourceProvider Provider, byte[] DocumentKeyHash, byte[] DocumentHash, byte[] DetailKeyHash, byte[] DetailHash, bool CompleteDocumentSnapshot);

public interface IClock { DateTime UtcNow { get; } }
public interface IHashService { byte[] Compute(params object?[] values); }
public record DataSourceCredential(string UserName, string Password);
public interface IDataSourceSecretResolver { DataSourceCredential Resolve(string secretReference); }
public interface IStationSaleExtractor
{
    SourceProvider Provider { get; }
    bool ProvidesCompleteDocumentSnapshot { get; }
    Task<IReadOnlyCollection<ExtractedSaleRow>> ExtractAsync(DataSource source, ExtractionWindow window, CancellationToken cancellationToken);
}
public interface IExtractorResolver { IStationSaleExtractor Resolve(SourceProvider provider); }
public interface IRawSaleWriter { Task<int> WriteAsync(ExtractionRun run, DataSource source, IReadOnlyCollection<ExtractedSaleRow> rows, CancellationToken cancellationToken); }
public interface IRawWriteCoordinator
{
    Task<T> ExecuteAsync<T>(SourceProvider provider, Func<CancellationToken, Task<T>> operation, CancellationToken cancellationToken);
}
public interface IExtractionRepository
{
    Task<IReadOnlyCollection<PipelineDataSource>> GetActiveAsync(string? sourceCode, CancellationToken cancellationToken);
    Task<ExtractionRun> StartAsync(PipelineDataSource configuration, ExtractionWindow window, CancellationToken cancellationToken);
    Task SucceedAsync(ExtractionRun run, PipelineDataSource configuration, int read, int written, DateTime checkpointUtc, CancellationToken cancellationToken);
    Task SucceedWithoutCheckpointAsync(ExtractionRun run, int read, int written, CancellationToken cancellationToken);
    Task SucceedWithNoDataAsync(ExtractionRun run, CancellationToken cancellationToken);
    Task FailAsync(ExtractionRun run, Exception exception, CancellationToken cancellationToken);
}
public interface ICoreRepository
{
    Task<SaleDocument?> FindDocumentAsync(byte[] key, CancellationToken cancellationToken);
    Task AddDocumentAsync(SaleDocument document, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
public interface IMartPublisher { PublishDestination Destination { get; } Task PublishAsync(CancellationToken cancellationToken); }
