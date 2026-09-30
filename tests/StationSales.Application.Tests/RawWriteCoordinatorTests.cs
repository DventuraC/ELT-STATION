using StationSales.Application;
using StationSales.Domain;

namespace StationSales.Application.Tests;

public sealed class RawWriteCoordinatorTests
{
    [Fact]
    public async Task Same_provider_writes_are_serialized()
    {
        using var coordinator = new RawWriteCoordinator();
        var firstEntered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var releaseFirst = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var secondEntered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        var first = coordinator.ExecuteAsync(SourceProvider.OpenComb, async _ =>
        {
            firstEntered.SetResult();
            await releaseFirst.Task;
            return 1;
        }, CancellationToken.None);
        await firstEntered.Task;

        var second = coordinator.ExecuteAsync(SourceProvider.OpenComb, _ =>
        {
            secondEntered.SetResult();
            return Task.FromResult(2);
        }, CancellationToken.None);

        await Task.Delay(100);
        Assert.False(secondEntered.Task.IsCompleted);
        releaseFirst.SetResult();

        Assert.Equal(new[] { 1, 2 }, await Task.WhenAll(first, second));
        Assert.True(secondEntered.Task.IsCompleted);
    }

    [Fact]
    public async Task Different_providers_can_write_concurrently()
    {
        using var coordinator = new RawWriteCoordinator();
        var openCombEntered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var releaseOpenComb = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var gasolutionEntered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        var openComb = coordinator.ExecuteAsync(SourceProvider.OpenComb, async _ =>
        {
            openCombEntered.SetResult();
            await releaseOpenComb.Task;
            return 1;
        }, CancellationToken.None);
        await openCombEntered.Task;

        var gasolution = coordinator.ExecuteAsync(SourceProvider.Gasolution, _ =>
        {
            gasolutionEntered.SetResult();
            return Task.FromResult(2);
        }, CancellationToken.None);

        await gasolutionEntered.Task.WaitAsync(TimeSpan.FromSeconds(1));
        releaseOpenComb.SetResult();

        Assert.Equal(new[] { 1, 2 }, await Task.WhenAll(openComb, gasolution));
    }
}
