using StationSales.Infrastructure;
namespace StationSales.Application.Tests;
public sealed class HashingTests
{
    [Fact] public void Hashing_is_deterministic() { var service = new DeterministicSha256HashService(); var a = service.Compute(" invoice ", 12.5m, new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), null); var b = service.Compute("invoice", 12.5m, new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), null); Assert.Equal(a, b); }
}
