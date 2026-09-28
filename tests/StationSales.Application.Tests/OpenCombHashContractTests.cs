using StationSales.Infrastructure;

namespace StationSales.Application.Tests;

public sealed class OpenCombHashContractTests
{
    [Fact]
    public void Quantity_changes_content_but_not_stable_keys()
    {
        var contract = new OpenCombHashContract(new DeterministicSha256HashService());
        var document = contract.DocumentKeyHash(16, "001", "F", "A", 1234m);
        var detail = contract.DetailKeyHash(document, "P001");
        var first = contract.DetailHash(detail, 10m, 5m, 1m, 11m);
        var corrected = contract.DetailHash(detail, 12m, 5m, 1m, 13m);

        Assert.NotEqual(first, corrected);
        Assert.Equal(detail, contract.DetailKeyHash(document, "P001"));
    }
}
