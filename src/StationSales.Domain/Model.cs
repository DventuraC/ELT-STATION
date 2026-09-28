namespace StationSales.Domain;

public enum SourceProvider { OpenComb, Gasolution }
public enum DatabaseType { PostgreSql, SqlServer }
public enum RunStatus { Pending, Running, Succeeded, SucceededWithNoData, Failed }
public enum PipelineType { Extract, Transform, Publish }
public enum PublishDestination { SqlServer, PostgreSql }

public sealed class Station
{
    public long Id { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string BusinessRuc { get; set; } = null!;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public ICollection<DataSource> DataSources { get; } = new List<DataSource>();
}

public sealed class SourceSystem
{
    public long Id { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public bool IsActive { get; set; } = true;
    public ICollection<DataSource> DataSources { get; } = new List<DataSource>();
}

public sealed class DataSource
{
    public long Id { get; set; }
    public long StationId { get; set; }
    public Station Station { get; set; } = null!;
    public long SourceSystemId { get; set; }
    public SourceSystem SourceSystem { get; set; } = null!;
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string StationCode { get; set; } = null!;
    public SourceProvider Provider { get; set; }
    public DatabaseType DatabaseType { get; set; }
    public string Server { get; set; } = null!;
    public int Port { get; set; }
    public string DatabaseName { get; set; } = null!;
    public string? Username { get; set; }
    public string SecretReference { get; set; } = null!;
    public string Environment { get; set; } = "Production";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}

public sealed class Pipeline
{
    public long Id { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public PipelineType PipelineType { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class PipelineDataSource
{
    public long Id { get; set; }
    public long PipelineId { get; set; }
    public long DataSourceId { get; set; }
    public Pipeline Pipeline { get; set; } = null!;
    public DataSource DataSource { get; set; } = null!;
    public bool IsActive { get; set; } = true;
    public DateTime StartDate { get; set; }
    public DateTime? LastSuccessfulDate { get; private set; }
    public int RecoveryDays { get; set; }
    public int IntervalMinutes { get; set; } = 60;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public void ConfirmCheckpoint(DateTime checkpointUtc) { LastSuccessfulDate = checkpointUtc; UpdatedAtUtc = DateTime.UtcNow; }
}

public sealed class ExtractionRun
{
    public long Id { get; set; }
    public long PipelineDataSourceId { get; set; }
    public long DataSourceId { get; set; }
    public DateTime WindowFrom { get; set; }
    public DateTime WindowTo { get; set; }
    public DateTime StartedAtUtc { get; set; }
    public DateTime? FinishedAtUtc { get; private set; }
    public RunStatus Status { get; private set; } = RunStatus.Running;
    public int RowsRead { get; private set; }
    public int RowsWritten { get; private set; }
    public string? ErrorCode { get; private set; }
    public string? ErrorMessage { get; private set; }
    public DateTime CreatedAtUtc { get; set; }
    public void Succeed(int rowsRead, int rowsWritten, DateTime finishedAtUtc) { Status = RunStatus.Succeeded; RowsRead = rowsRead; RowsWritten = rowsWritten; FinishedAtUtc = finishedAtUtc; }
    public void SucceedWithNoData(DateTime finishedAtUtc) { Status = RunStatus.SucceededWithNoData; RowsRead = 0; RowsWritten = 0; FinishedAtUtc = finishedAtUtc; }
    public void Fail(string code, string message, DateTime finishedAtUtc) { Status = RunStatus.Failed; ErrorCode = code; ErrorMessage = message; FinishedAtUtc = finishedAtUtc; }
}

public sealed class TransformBatch
{
    public long Id { get; set; }
    public long PipelineId { get; set; }
    public DateTime StartedAtUtc { get; set; }
    public DateTime? FinishedAtUtc { get; set; }
    public RunStatus Status { get; set; } = RunStatus.Running;
    public int DocumentsProcessed { get; set; }
    public int DetailsProcessed { get; set; }
    public int InsertedCount { get; set; }
    public int UpdatedCount { get; set; }
    public int UnchangedCount { get; set; }
    public int SoftDeletedCount { get; set; }
    public string? ErrorMessage { get; set; }
    public ICollection<TransformBatchExtraction> Extractions { get; } = new List<TransformBatchExtraction>();
}
public sealed class TransformBatchExtraction { public long TransformBatchId { get; set; } public long ExtractionRunId { get; set; } }

public sealed class PublishRun
{
    public long Id { get; set; }
    public PublishDestination Destination { get; set; }
    public DateTime StartedAtUtc { get; set; }
    public DateTime? FinishedAtUtc { get; set; }
    public RunStatus Status { get; set; }
    public string? Checkpoint { get; set; }
    public int RowsProcessed { get; set; }
    public int RowsInserted { get; set; }
    public int RowsUpdated { get; set; }
    public string? ErrorMessage { get; set; }
}

public sealed class SaleDocument
{
    public long Id { get; set; }
    public byte[] DocumentKeyHash { get; private set; } = null!;
    public byte[] DocumentHash { get; private set; } = null!;
    public SourceProvider SourceProvider { get; private set; }
    public string StationCode { get; private set; } = null!;
    public long LastSeenExtractionRunId { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; private set; }
    public DateTime? DeletedAtUtc { get; private set; }
    public ICollection<SaleDocumentDetail> Details { get; } = new List<SaleDocumentDetail>();
    public SaleDocument(byte[] key, byte[] hash, SourceProvider provider, string station, long runId, DateTime now) { DocumentKeyHash = key; DocumentHash = hash; SourceProvider = provider; StationCode = station; LastSeenExtractionRunId = runId; CreatedAtUtc = UpdatedAtUtc = now; }
    private SaleDocument() { }
    public bool Synchronize(byte[] hash, long runId, DateTime now) { var changed = !DocumentHash.SequenceEqual(hash); DocumentHash = hash; LastSeenExtractionRunId = runId; IsActive = true; DeletedAtUtc = null; if (changed) UpdatedAtUtc = now; return changed; }
}

public sealed class SaleDocumentDetail
{
    public long Id { get; set; }
    public long SaleDocumentId { get; set; }
    public byte[] DetailKeyHash { get; private set; } = null!;
    public byte[] DetailHash { get; private set; } = null!;
    public long LastSeenExtractionRunId { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; private set; }
    public DateTime? DeletedAtUtc { get; private set; }
    public SaleDocumentDetail(byte[] key, byte[] hash, long runId, DateTime now) { DetailKeyHash = key; DetailHash = hash; LastSeenExtractionRunId = runId; CreatedAtUtc = UpdatedAtUtc = now; }
    private SaleDocumentDetail() { }
    public bool Synchronize(byte[] hash, long runId, DateTime now) { var changed = !DetailHash.SequenceEqual(hash); DetailHash = hash; LastSeenExtractionRunId = runId; IsActive = true; DeletedAtUtc = null; if (changed) UpdatedAtUtc = now; return changed; }
    public void SoftDelete(DateTime now) { IsActive = false; DeletedAtUtc = now; UpdatedAtUtc = now; }
}
