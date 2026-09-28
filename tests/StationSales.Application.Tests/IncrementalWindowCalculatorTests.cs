using StationSales.Application;
using StationSales.Domain;
namespace StationSales.Application.Tests;
public sealed class IncrementalWindowCalculatorTests
{
    [Fact] public void First_run_starts_before_configured_start_date() { var c = new PipelineDataSource { StartDate = new DateTime(2026, 6, 1), RecoveryDays = 3 }; var result = new IncrementalWindowCalculator().Calculate(c, new DateTime(2026, 9, 26)); Assert.Equal(new DateTime(2026, 5, 29), result.FromUtc); }
    [Fact] public void Negative_recovery_days_is_rejected() { var c = new PipelineDataSource { StartDate = DateTime.UtcNow, RecoveryDays = -1 }; Assert.Throws<ArgumentOutOfRangeException>(() => new IncrementalWindowCalculator().Calculate(c, DateTime.UtcNow)); }
}
