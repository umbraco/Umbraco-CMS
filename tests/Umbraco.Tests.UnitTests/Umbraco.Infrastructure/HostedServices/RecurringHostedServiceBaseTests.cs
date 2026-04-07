// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Time.Testing;
using NUnit.Framework;
using Umbraco.Cms.Infrastructure.HostedServices;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.HostedServices;

[TestFixture]
public class RecurringHostedServiceBaseTests
{
    [TestCase(10_000, 3_000, 7_000, Description = "Subtracts elapsed time from period")]
    [TestCase(10_000, 15_000, 0, Description = "Returns zero when execution exceeds period")]
    [TestCase(10_000, 0, 10_000, Description = "Returns full period when elapsed is zero")]
    [TestCase(10_000, 10_000, 0, Description = "Returns zero when execution equals period")]
    [TestCase(-1, 1_000, 0, Description = "Returns zero for negative period")]
    public void ComputeNextDelay_Returns_Expected_Result(long periodMs, long elapsedMs, long expectedMs)
    {
        var period = TimeSpan.FromMilliseconds(periodMs);
        var elapsed = TimeSpan.FromMilliseconds(elapsedMs);

        TimeSpan result = RecurringHostedServiceBase.ComputeNextDelay(period, elapsed);

        Assert.AreEqual(TimeSpan.FromMilliseconds(expectedMs), result);
    }

    [Test]
    public async Task Loop_Executes_Periodically_And_Respects_Cancellation()
    {
        var executionCount = 0;
        var timeProvider = new FakeTimeProvider();
        using var executed = new SemaphoreSlim(0);
        var sut = new TestRecurringHostedService(
            period: TimeSpan.FromMinutes(5),
            delay: TimeSpan.Zero,
            timeProvider: timeProvider,
            onExecute: _ => { Interlocked.Increment(ref executionCount); executed.Release(); return Task.CompletedTask; });

        using var cts = new CancellationTokenSource();
        await sut.StartAsync(cts.Token);

        Assert.IsTrue(await executed.WaitAsync(TimeSpan.FromSeconds(5)), "First execution should complete");
        Assert.AreEqual(1, executionCount);

        timeProvider.Advance(TimeSpan.FromMinutes(5));
        Assert.IsTrue(await executed.WaitAsync(TimeSpan.FromSeconds(5)), "Second execution should complete");
        Assert.AreEqual(2, executionCount);

        timeProvider.Advance(TimeSpan.FromMinutes(5));
        Assert.IsTrue(await executed.WaitAsync(TimeSpan.FromSeconds(5)), "Third execution should complete");
        Assert.AreEqual(3, executionCount);

        cts.Cancel();
        await sut.StopAsync(CancellationToken.None);
    }

    [Test]
    public async Task TriggerExecution_Causes_Immediate_Execution()
    {
        var executionCount = 0;
        var timeProvider = new FakeTimeProvider();
        using var executed = new SemaphoreSlim(0);
        var sut = new TestRecurringHostedService(
            period: TimeSpan.FromHours(1),
            delay: TimeSpan.Zero,
            timeProvider: timeProvider,
            onExecute: _ => { Interlocked.Increment(ref executionCount); executed.Release(); return Task.CompletedTask; });

        using var cts = new CancellationTokenSource();
        await sut.StartAsync(cts.Token);

        Assert.IsTrue(await executed.WaitAsync(TimeSpan.FromSeconds(5)), "First execution should complete");
        Assert.AreEqual(1, executionCount, "Should have executed once initially");

        sut.PublicTriggerExecution();
        Assert.IsTrue(await executed.WaitAsync(TimeSpan.FromSeconds(5)), "Triggered execution should complete");
        Assert.AreEqual(2, executionCount, "Should have executed again after trigger");

        cts.Cancel();
        await sut.StopAsync(CancellationToken.None);
    }

    [Test]
    public async Task TriggerExecution_Reset_Starts_New_Full_Period()
    {
        var executionCount = 0;
        var timeProvider = new FakeTimeProvider();
        using var executed = new SemaphoreSlim(0);
        var sut = new TestRecurringHostedService(
            period: TimeSpan.FromHours(1),
            delay: TimeSpan.Zero,
            timeProvider: timeProvider,
            onExecute: _ => { Interlocked.Increment(ref executionCount); executed.Release(); return Task.CompletedTask; });

        using var cts = new CancellationTokenSource();
        await sut.StartAsync(cts.Token);

        Assert.IsTrue(await executed.WaitAsync(TimeSpan.FromSeconds(5)));
        Assert.AreEqual(1, executionCount);

        // Advance 30min into the 1h period, then trigger with Reset
        timeProvider.Advance(TimeSpan.FromMinutes(30));
        sut.PublicTriggerExecutionReset();
        Assert.IsTrue(await executed.WaitAsync(TimeSpan.FromSeconds(5)), "Triggered execution should complete");
        Assert.AreEqual(2, executionCount);

        // Reset means full period from now. Advancing 59min should not trigger.
        timeProvider.Advance(TimeSpan.FromMinutes(59));
        await Task.Yield();
        Assert.AreEqual(2, executionCount, "Should not execute before full period");

        // Advancing 1 more minute completes the period
        timeProvider.Advance(TimeSpan.FromMinutes(1));
        Assert.IsTrue(await executed.WaitAsync(TimeSpan.FromSeconds(5)));
        Assert.AreEqual(3, executionCount, "Should execute after full period");

        cts.Cancel();
        await sut.StopAsync(CancellationToken.None);
    }

    [Test]
    public async Task TriggerExecution_None_Resumes_Original_Wait()
    {
        var executionCount = 0;
        var timeProvider = new FakeTimeProvider();
        using var executed = new SemaphoreSlim(0);
        var sut = new TestRecurringHostedService(
            period: TimeSpan.FromHours(1),
            delay: TimeSpan.Zero,
            timeProvider: timeProvider,
            onExecute: _ => { Interlocked.Increment(ref executionCount); executed.Release(); return Task.CompletedTask; });

        using var cts = new CancellationTokenSource();
        await sut.StartAsync(cts.Token);

        Assert.IsTrue(await executed.WaitAsync(TimeSpan.FromSeconds(5)));
        Assert.AreEqual(1, executionCount);

        // Advance 20min into 1h period, then trigger with None
        timeProvider.Advance(TimeSpan.FromMinutes(20));
        sut.PublicTriggerExecutionNone();
        Assert.IsTrue(await executed.WaitAsync(TimeSpan.FromSeconds(5)), "Triggered execution should complete");
        Assert.AreEqual(2, executionCount);

        // None means resume original schedule. Remaining is ~40min. Advancing 39min should not trigger.
        timeProvider.Advance(TimeSpan.FromMinutes(39));
        await Task.Yield();
        Assert.AreEqual(2, executionCount, "Should not execute before original schedule");

        // Advancing 1 more minute reaches the original tick
        timeProvider.Advance(TimeSpan.FromMinutes(1));
        Assert.IsTrue(await executed.WaitAsync(TimeSpan.FromSeconds(5)));
        Assert.AreEqual(3, executionCount, "Should execute at original scheduled time");

        cts.Cancel();
        await sut.StopAsync(CancellationToken.None);
    }

    [Test]
    public async Task TriggerExecution_None_Skips_Overshot_Execution()
    {
        var executionCount = 0;
        var timeProvider = new FakeTimeProvider();
        using var executed = new SemaphoreSlim(0);
        var sut = new TestRecurringHostedService(
            period: TimeSpan.FromHours(1),
            delay: TimeSpan.Zero,
            timeProvider: timeProvider,
            onExecute: _ =>
            {
                var count = Interlocked.Increment(ref executionCount);
                if (count == 2)
                {
                    // Simulate a triggered execution that takes longer than the remaining time
                    timeProvider.Advance(TimeSpan.FromMinutes(50));
                }

                executed.Release();
                return Task.CompletedTask;
            });

        using var cts = new CancellationTokenSource();
        await sut.StartAsync(cts.Token);

        Assert.IsTrue(await executed.WaitAsync(TimeSpan.FromSeconds(5)));
        Assert.AreEqual(1, executionCount);

        // Advance 20min, then trigger with None. Remaining is 40min.
        // The triggered execution will advance time by 50min (overshooting by 10min).
        timeProvider.Advance(TimeSpan.FromMinutes(20));
        sut.PublicTriggerExecutionNone();
        Assert.IsTrue(await executed.WaitAsync(TimeSpan.FromSeconds(5)));
        Assert.AreEqual(2, executionCount);

        // The overshoot should skip the immediate tick. Next tick is at original + period = 2h from start.
        // We're now at ~70min. Advancing 49min (to ~119min) should not trigger.
        timeProvider.Advance(TimeSpan.FromMinutes(49));
        await Task.Yield();
        Assert.AreEqual(2, executionCount, "Should not execute — overshot tick was skipped");

        // Advancing 1 more minute reaches the next period tick
        timeProvider.Advance(TimeSpan.FromMinutes(1));
        Assert.IsTrue(await executed.WaitAsync(TimeSpan.FromSeconds(5)));
        Assert.AreEqual(3, executionCount, "Should execute at next period tick");

        cts.Cancel();
        await sut.StopAsync(CancellationToken.None);
    }

    [Test]
    public async Task TriggerExecution_Replace_Skips_Next_Tick()
    {
        var executionCount = 0;
        var timeProvider = new FakeTimeProvider();
        using var executed = new SemaphoreSlim(0);
        var sut = new TestRecurringHostedService(
            period: TimeSpan.FromHours(1),
            delay: TimeSpan.Zero,
            timeProvider: timeProvider,
            onExecute: _ => { Interlocked.Increment(ref executionCount); executed.Release(); return Task.CompletedTask; });

        using var cts = new CancellationTokenSource();
        await sut.StartAsync(cts.Token);

        Assert.IsTrue(await executed.WaitAsync(TimeSpan.FromSeconds(5)));
        Assert.AreEqual(1, executionCount);

        // Advance 20min into 1h period, then trigger with Replace.
        // Remaining is ~40min. Next execution at remaining + period = ~40min + 1h = ~100min from now.
        timeProvider.Advance(TimeSpan.FromMinutes(20));
        sut.PublicTriggerExecutionReplace();
        Assert.IsTrue(await executed.WaitAsync(TimeSpan.FromSeconds(5)), "Triggered execution should complete");
        Assert.AreEqual(2, executionCount);

        // The original next tick at 40min should be skipped. Advance to 60min — past the skipped tick.
        timeProvider.Advance(TimeSpan.FromMinutes(60));
        await Task.Yield();
        Assert.AreEqual(2, executionCount, "Should not execute — skipped scheduled tick");

        // Advance to the tick after the skipped one (~100min from trigger, ~40min more)
        timeProvider.Advance(TimeSpan.FromMinutes(40));
        Assert.IsTrue(await executed.WaitAsync(TimeSpan.FromSeconds(5)));
        Assert.AreEqual(3, executionCount, "Should execute at tick after the skipped one");

        cts.Cancel();
        await sut.StopAsync(CancellationToken.None);
    }

    [Test]
    public async Task TriggerExecution_CustomDelay_Uses_Specified_Delay()
    {
        var executionCount = 0;
        var timeProvider = new FakeTimeProvider();
        using var executed = new SemaphoreSlim(0);
        var sut = new TestRecurringHostedService(
            period: TimeSpan.FromHours(1),
            delay: TimeSpan.Zero,
            timeProvider: timeProvider,
            onExecute: _ => { Interlocked.Increment(ref executionCount); executed.Release(); return Task.CompletedTask; });

        using var cts = new CancellationTokenSource();
        await sut.StartAsync(cts.Token);

        Assert.IsTrue(await executed.WaitAsync(TimeSpan.FromSeconds(5)));
        Assert.AreEqual(1, executionCount);

        // Trigger with a custom 10-minute delay
        sut.PublicTriggerExecutionWithDelay(TimeSpan.FromMinutes(10));
        Assert.IsTrue(await executed.WaitAsync(TimeSpan.FromSeconds(5)), "Triggered execution should complete");
        Assert.AreEqual(2, executionCount);

        // After the triggered execution, next should come after the custom 10min delay
        timeProvider.Advance(TimeSpan.FromMinutes(9));
        await Task.Yield();
        Assert.AreEqual(2, executionCount, "Should not execute before custom delay");

        timeProvider.Advance(TimeSpan.FromMinutes(1));
        Assert.IsTrue(await executed.WaitAsync(TimeSpan.FromSeconds(5)));
        Assert.AreEqual(3, executionCount, "Should execute after custom delay");

        cts.Cancel();
        await sut.StopAsync(CancellationToken.None);
    }

    [Test]
    public async Task TriggerExecution_During_InitialDelay_Does_Not_Leak_Strategy()
    {
        var executionCount = 0;
        var timeProvider = new FakeTimeProvider();
        using var executed = new SemaphoreSlim(0);
        var sut = new TestRecurringHostedService(
            period: TimeSpan.FromHours(1),
            delay: TimeSpan.FromMinutes(30),
            timeProvider: timeProvider,
            onExecute: _ => { Interlocked.Increment(ref executionCount); executed.Release(); return Task.CompletedTask; });

        using var cts = new CancellationTokenSource();
        await sut.StartAsync(cts.Token);

        // Still in initial delay — no execution yet
        await Task.Yield();
        Assert.AreEqual(0, executionCount, "Should not have executed during initial delay");

        // Trigger with a custom delay during the initial delay
        sut.PublicTriggerExecutionWithDelay(TimeSpan.FromMinutes(5));
        Assert.IsTrue(await executed.WaitAsync(TimeSpan.FromSeconds(5)));
        Assert.AreEqual(1, executionCount, "Should have executed once after trigger interrupted delay");

        // The custom delay should NOT be applied after the first execution —
        // it was consumed when the initial delay was interrupted.
        // Next wait should use the normal 1h period.
        timeProvider.Advance(TimeSpan.FromMinutes(59));
        await Task.Yield();
        Assert.AreEqual(1, executionCount, "Custom delay should not leak — next wait uses normal period");

        timeProvider.Advance(TimeSpan.FromMinutes(1));
        Assert.IsTrue(await executed.WaitAsync(TimeSpan.FromSeconds(5)));
        Assert.AreEqual(2, executionCount, "Should execute after normal period");

        cts.Cancel();
        await sut.StopAsync(CancellationToken.None);
    }

    [Test]
    public async Task Exception_In_PerformExecuteAsync_Does_Not_Kill_Loop()
    {
        var executionCount = 0;
        var timeProvider = new FakeTimeProvider();
        using var executed = new SemaphoreSlim(0);
        var sut = new TestRecurringHostedService(
            period: TimeSpan.FromMinutes(5),
            delay: TimeSpan.Zero,
            timeProvider: timeProvider,
            onExecute: _ =>
            {
                var count = Interlocked.Increment(ref executionCount);
                executed.Release();
                if (count == 1)
                {
                    // Advance past the period so the next execution fires immediately
                    // after the loop catches the exception (no wait needed).
                    timeProvider.Advance(TimeSpan.FromMinutes(5));
                    throw new InvalidOperationException("Test exception");
                }
                return Task.CompletedTask;
            });

        using var cts = new CancellationTokenSource();
        await sut.StartAsync(cts.Token);

        Assert.IsTrue(await executed.WaitAsync(TimeSpan.FromSeconds(5)), "First execution (throws) should complete");
        Assert.AreEqual(1, executionCount);

        Assert.IsTrue(await executed.WaitAsync(TimeSpan.FromSeconds(5)), "Second execution should complete despite first throwing");
        Assert.AreEqual(2, executionCount, "Loop should continue after exception");

        cts.Cancel();
        await sut.StopAsync(CancellationToken.None);
    }

    /// <summary>
    /// A concrete test subclass that overrides the new PerformExecuteAsync(CancellationToken).
    /// </summary>
    private class TestRecurringHostedService : RecurringHostedServiceBase
    {
        private readonly Func<CancellationToken, Task> _onExecute;

        public TestRecurringHostedService(TimeSpan period, TimeSpan delay, TimeProvider timeProvider, Func<CancellationToken, Task> onExecute)
            : base(null, period, delay, timeProvider)
        {
            _onExecute = onExecute;
        }

        public override Task PerformExecuteAsync(CancellationToken stoppingToken)
            => _onExecute(stoppingToken);

        public void PublicTriggerExecution() => TriggerExecution();

        public void PublicTriggerExecutionReset() => TriggerExecution(NextExecutionStrategy.Reset);

        public void PublicTriggerExecutionNone() => TriggerExecution(NextExecutionStrategy.None);

        public void PublicTriggerExecutionReplace() => TriggerExecution(NextExecutionStrategy.Replace);

        public void PublicTriggerExecutionWithDelay(TimeSpan nextDelay) => TriggerExecution(nextDelay);

        public void PublicChangePeriod(TimeSpan newPeriod) => ChangePeriod(newPeriod);
    }
}
