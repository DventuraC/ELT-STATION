using StationSales.Application;
using StationSales.Domain;
namespace StationSales.Application.Tests;
public sealed class IncrementalWindowCalculatorTests
{
    [Fact] public void First_run_never_starts_before_configured_start_date() { var c = new PipelineDataSource { StartDate = new DateTime(2026, 9, 1), RecoveryDays = 5 }; var result = new IncrementalWindowCalculator().Calculate(c, new DateTime(2026, 9, 25)); Assert.Equal(new DateTime(2026, 9, 1), result.FromUtc); }
    [Fact] public void Recovery_window_is_applied_after_a_successful_checkpoint() { var c = new PipelineDataSource { StartDate = new DateTime(2026, 9, 1), RecoveryDays = 5 }; c.ConfirmCheckpoint(new DateTime(2026, 9, 20)); var result = new IncrementalWindowCalculator().Calculate(c, new DateTime(2026, 9, 25)); Assert.Equal(new DateTime(2026, 9, 15), result.FromUtc); }
    [Fact] public void Negative_recovery_days_is_rejected() { var c = new PipelineDataSource { StartDate = DateTime.UtcNow, RecoveryDays = -1 }; Assert.Throws<ArgumentOutOfRangeException>(() => new IncrementalWindowCalculator().Calculate(c, DateTime.UtcNow)); }
}
