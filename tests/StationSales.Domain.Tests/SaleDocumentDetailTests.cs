using StationSales.Domain;
namespace StationSales.Domain.Tests;
public sealed class SaleDocumentDetailTests
{
    [Fact] public void Reactivates_a_soft_deleted_detail() { var key = new byte[32]; var first = new byte[32]; var changed = Enumerable.Repeat((byte)1, 32).ToArray(); var detail = new SaleDocumentDetail(key, first, 1, DateTime.UtcNow); detail.SoftDelete(DateTime.UtcNow); var isChanged = detail.Synchronize(changed, 2, DateTime.UtcNow); Assert.True(isChanged); Assert.True(detail.IsActive); Assert.Null(detail.DeletedAtUtc); Assert.Equal(2, detail.LastSeenExtractionRunId); }
}
