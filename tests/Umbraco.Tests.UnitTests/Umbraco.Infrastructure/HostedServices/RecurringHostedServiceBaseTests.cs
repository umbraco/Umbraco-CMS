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
    [TestCase(-2, 1_000, 0, Description = "Returns zero for negative (non-infinite) period")]
    [TestCase(-1, 1_000, -1, Description = "Preserves Timeout.InfiniteTimeSpan")]
    [TestCase(-1, 0, -1, Description = "Preserves Timeout.InfiniteTimeSpan when elapsed is zero")]
    public void ComputeNextDelay_Returns_Expected_Result(long periodMs, long elapsedMs, long expectedMs)
    {
        var period = TimeSpan.FromMilliseconds(periodMs);
        var elapsed = TimeSpan.FromMilliseconds(elapsedMs);

        TimeSpan result = RecurringHostedServiceBase.ComputeNextDelay(period, elapsed);

        Assert.That(result, Is.EqualTo(TimeSpan.FromMilliseconds(expectedMs)));
    }

    [TestCase(0)]
    [TestCase(-2)]
    public void Constructor_Throws_For_Negative_NonInfinite_Period(long periodMs)
    {
        if (periodMs == 0)
        {
            Assert.DoesNotThrow(() => _ = new TestRecurringHostedService(TimeSpan.Zero, TimeSpan.Zero, new FakeTimeProvider(), _ => Task.CompletedTask));
        }
        else
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _ = new TestRecurringHostedService(TimeSpan.FromMilliseconds(periodMs), TimeSpan.Zero, new FakeTimeProvider(), _ => Task.CompletedTask));
        }
    }

    [Test]
    public void Constructor_Allows_Infinite_Period()
    {
        Assert.DoesNotThrow(() => _ = new TestRecurringHostedService(Timeout.InfiniteTimeSpan, TimeSpan.Zero, new FakeTimeProvider(), _ => Task.CompletedTask));
    }

    [Test]
    public void Constructor_Allows_Infinite_Delay()
    {
        Assert.DoesNotThrow(() => _ = new TestRecurringHostedService(TimeSpan.FromMinutes(1), Timeout.InfiniteTimeSpan, new FakeTimeProvider(), _ => Task.CompletedTask));
    }

    [Test]
    public void ChangePeriod_Allows_Infinite()
    {
        var sut = new TestRecurringHostedService(TimeSpan.FromMinutes(1), TimeSpan.Zero, new FakeTimeProvider(), _ => Task.CompletedTask);

        Assert.DoesNotThrow(() => sut.PublicChangePeriod(Timeout.InfiniteTimeSpan));
    }

    [Test]
    public void TriggerExecution_NextDelay_Allows_Infinite()
    {
        var sut = new TestRecurringHostedService(TimeSpan.FromMinutes(1), TimeSpan.Zero, new FakeTimeProvider(), _ => Task.CompletedTask);

        Assert.DoesNotThrow(() => sut.PublicTriggerExecutionWithDelay(Timeout.InfiniteTimeSpan));
    }

    [Test]
    public async Task Infinite_Delay_Skips_Initial_Execution_Until_Triggered()
    {
        var executionCount = 0;
        var timeProvider = new FakeTimeProvider();
        using var executed = new SemaphoreSlim(0);
        var sut = new TestRecurringHostedService(
            period: TimeSpan.FromMinutes(10),
            delay: Timeout.InfiniteTimeSpan,
            timeProvider: timeProvider,
            onExecute: _ => { Interlocked.Increment(ref executionCount); executed.Release(); return Task.CompletedTask; });

        using var cts = new CancellationTokenSource();
        await sut.StartAsync(cts.Token);

        // Advancing time does not fire the first execution — initial delay is infinite.
        timeProvider.Advance(TimeSpan.FromHours(24));
        Assert.That(await executed.WaitAsync(TimeSpan.FromMilliseconds(50)), Is.False, "Should not execute on its own when initial delay is infinite");

        // Manual trigger fires the first execution.
        sut.PublicTriggerExecution();
        Assert.That(await executed.WaitAsync(TimeSpan.FromSeconds(5)), Is.True, "Triggered first execution should complete");
        Assert.That(executionCount, Is.EqualTo(1));

        // After the first execution, the normal period applies — advance 10 min to fire the second.
        timeProvider.Advance(TimeSpan.FromMinutes(10));
        Assert.That(await executed.WaitAsync(TimeSpan.FromSeconds(5)), Is.True, "Second execution should fire on Period after the triggered first run");
        Assert.That(executionCount, Is.EqualTo(2));

        cts.Cancel();
        await sut.StopAsync(CancellationToken.None);
    }

    [Test]
    public async Task Infinite_Delay_And_Infinite_Period_Runs_Only_When_Triggered()
    {
        var executionCount = 0;
        var timeProvider = new FakeTimeProvider();
        using var executed = new SemaphoreSlim(0);
        var sut = new TestRecurringHostedService(
            period: Timeout.InfiniteTimeSpan,
            delay: Timeout.InfiniteTimeSpan,
            timeProvider: timeProvider,
            onExecute: _ => { Interlocked.Increment(ref executionCount); executed.Release(); return Task.CompletedTask; });

        using var cts = new CancellationTokenSource();
        await sut.StartAsync(cts.Token);

        timeProvider.Advance(TimeSpan.FromHours(24));
        Assert.That(await executed.WaitAsync(TimeSpan.FromMilliseconds(50)), Is.False, "Should not execute without a trigger");

        sut.PublicTriggerExecution();
        Assert.That(await executed.WaitAsync(TimeSpan.FromSeconds(5)), Is.True);
        Assert.That(executionCount, Is.EqualTo(1));

        // Period is infinite, so no further executions without another trigger.
        timeProvider.Advance(TimeSpan.FromHours(24));
        Assert.That(await executed.WaitAsync(TimeSpan.FromMilliseconds(50)), Is.False, "Should not execute again without a trigger");

        sut.PublicTriggerExecution();
        Assert.That(await executed.WaitAsync(TimeSpan.FromSeconds(5)), Is.True);
        Assert.That(executionCount, Is.EqualTo(2));

        cts.Cancel();
        await sut.StopAsync(CancellationToken.None);
    }

    [Test]
    public async Task Infinite_Period_Waits_For_Trigger_Only()
    {
        var executionCount = 0;
        var timeProvider = new FakeTimeProvider();
        using var executed = new SemaphoreSlim(0);
        var sut = new TestRecurringHostedService(
            period: Timeout.InfiniteTimeSpan,
            delay: TimeSpan.Zero,
            timeProvider: timeProvider,
            onExecute: _ => { Interlocked.Increment(ref executionCount); executed.Release(); return Task.CompletedTask; });

        using var cts = new CancellationTokenSource();
        await sut.StartAsync(cts.Token);

        // First execution fires (delay = Zero — no initial wait)
        Assert.That(await executed.WaitAsync(TimeSpan.FromSeconds(5)), Is.True, "Initial execution should complete");
        Assert.That(executionCount, Is.EqualTo(1));

        // Time advancing should not trigger another execution
        timeProvider.Advance(TimeSpan.FromHours(24));
        Assert.That(await executed.WaitAsync(TimeSpan.FromMilliseconds(50)), Is.False, "Should not execute on its own when period is infinite");

        // Manual trigger fires the next execution
        sut.PublicTriggerExecution();
        Assert.That(await executed.WaitAsync(TimeSpan.FromSeconds(5)), Is.True, "Triggered execution should complete");
        Assert.That(executionCount, Is.EqualTo(2));

        // After the trigger, the next cycle should still wait for another trigger
        timeProvider.Advance(TimeSpan.FromHours(24));
        Assert.That(await executed.WaitAsync(TimeSpan.FromMilliseconds(50)), Is.False, "Should resume infinite wait after trigger");

        cts.Cancel();
        await sut.StopAsync(CancellationToken.None);
    }

    [Test]
    public async Task ChangePeriod_From_Infinite_To_Finite_Resumes_Scheduling()
    {
        var executionCount = 0;
        var timeProvider = new FakeTimeProvider();
        using var executed = new SemaphoreSlim(0);
        var sut = new TestRecurringHostedService(
            period: Timeout.InfiniteTimeSpan,
            delay: TimeSpan.Zero,
            timeProvider: timeProvider,
            onExecute: _ => { Interlocked.Increment(ref executionCount); executed.Release(); return Task.CompletedTask; });

        using var cts = new CancellationTokenSource();
        await sut.StartAsync(cts.Token);

        Assert.That(await executed.WaitAsync(TimeSpan.FromSeconds(5)), Is.True);
        Assert.That(executionCount, Is.EqualTo(1));

        // Switch to a 10-minute period — the in-flight infinite wait should be interrupted.
        sut.PublicChangePeriod(TimeSpan.FromMinutes(10));

        timeProvider.Advance(TimeSpan.FromMinutes(10));
        Assert.That(await executed.WaitAsync(TimeSpan.FromSeconds(5)), Is.True, "Should execute on schedule after switching to finite period");
        Assert.That(executionCount, Is.EqualTo(2));

        // Switch back to infinite — schedule disabled again.
        sut.PublicChangePeriod(Timeout.InfiniteTimeSpan);
        timeProvider.Advance(TimeSpan.FromHours(24));
        Assert.That(await executed.WaitAsync(TimeSpan.FromMilliseconds(50)), Is.False, "Should not execute after switching back to infinite");

        cts.Cancel();
        await sut.StopAsync(CancellationToken.None);
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

        Assert.That(await executed.WaitAsync(TimeSpan.FromSeconds(5)), Is.True, "First execution should complete");
        Assert.That(executionCount, Is.EqualTo(1));

        timeProvider.Advance(TimeSpan.FromMinutes(5));
        Assert.That(await executed.WaitAsync(TimeSpan.FromSeconds(5)), Is.True, "Second execution should complete");
        Assert.That(executionCount, Is.EqualTo(2));

        timeProvider.Advance(TimeSpan.FromMinutes(5));
        Assert.That(await executed.WaitAsync(TimeSpan.FromSeconds(5)), Is.True, "Third execution should complete");
        Assert.That(executionCount, Is.EqualTo(3));

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

        Assert.That(await executed.WaitAsync(TimeSpan.FromSeconds(5)), Is.True, "First execution should complete");
        Assert.That(executionCount, Is.EqualTo(1), "Should have executed once initially");

        sut.PublicTriggerExecution();
        Assert.That(await executed.WaitAsync(TimeSpan.FromSeconds(5)), Is.True, "Triggered execution should complete");
        Assert.That(executionCount, Is.EqualTo(2), "Should have executed again after trigger");

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

        Assert.That(await executed.WaitAsync(TimeSpan.FromSeconds(5)), Is.True);
        Assert.That(executionCount, Is.EqualTo(1));

        // Advance 30min into the 1h period, then trigger with Reset
        timeProvider.Advance(TimeSpan.FromMinutes(30));
        sut.PublicTriggerExecutionReset();
        Assert.That(await executed.WaitAsync(TimeSpan.FromSeconds(5)), Is.True, "Triggered execution should complete");
        Assert.That(executionCount, Is.EqualTo(2));

        // Reset means full period from now. Advancing 59min should not trigger.
        timeProvider.Advance(TimeSpan.FromMinutes(59));
        Assert.That(await executed.WaitAsync(TimeSpan.FromMilliseconds(50)), Is.False, "Should not execute before full period");

        // Advancing 1 more minute completes the period
        timeProvider.Advance(TimeSpan.FromMinutes(1));
        Assert.That(await executed.WaitAsync(TimeSpan.FromSeconds(5)), Is.True);
        Assert.That(executionCount, Is.EqualTo(3), "Should execute after full period");

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

        Assert.That(await executed.WaitAsync(TimeSpan.FromSeconds(5)), Is.True);
        Assert.That(executionCount, Is.EqualTo(1));

        // Advance 20min into 1h period, then trigger with None
        timeProvider.Advance(TimeSpan.FromMinutes(20));
        sut.PublicTriggerExecutionNone();
        Assert.That(await executed.WaitAsync(TimeSpan.FromSeconds(5)), Is.True, "Triggered execution should complete");
        Assert.That(executionCount, Is.EqualTo(2));

        // None means resume original schedule. Remaining is ~40min. Advancing 39min should not trigger.
        timeProvider.Advance(TimeSpan.FromMinutes(39));
        Assert.That(await executed.WaitAsync(TimeSpan.FromMilliseconds(50)), Is.False, "Should not execute before original schedule");

        // Advancing 1 more minute reaches the original tick
        timeProvider.Advance(TimeSpan.FromMinutes(1));
        Assert.That(await executed.WaitAsync(TimeSpan.FromSeconds(5)), Is.True);
        Assert.That(executionCount, Is.EqualTo(3), "Should execute at original scheduled time");

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

        Assert.That(await executed.WaitAsync(TimeSpan.FromSeconds(5)), Is.True);
        Assert.That(executionCount, Is.EqualTo(1));

        // Advance 20min, then trigger with None. Remaining is 40min.
        // The triggered execution will advance time by 50min (overshooting by 10min).
        timeProvider.Advance(TimeSpan.FromMinutes(20));
        sut.PublicTriggerExecutionNone();
        Assert.That(await executed.WaitAsync(TimeSpan.FromSeconds(5)), Is.True);
        Assert.That(executionCount, Is.EqualTo(2));

        // The overshoot should skip the immediate tick. Next tick is at original + period = 2h from start.
        // We're now at ~70min. Advancing 49min (to ~119min) should not trigger.
        timeProvider.Advance(TimeSpan.FromMinutes(49));
        Assert.That(await executed.WaitAsync(TimeSpan.FromMilliseconds(50)), Is.False, "Should not execute — overshot tick was skipped");

        // Advancing 1 more minute reaches the next period tick
        timeProvider.Advance(TimeSpan.FromMinutes(1));
        Assert.That(await executed.WaitAsync(TimeSpan.FromSeconds(5)), Is.True);
        Assert.That(executionCount, Is.EqualTo(3), "Should execute at next period tick");

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

        Assert.That(await executed.WaitAsync(TimeSpan.FromSeconds(5)), Is.True);
        Assert.That(executionCount, Is.EqualTo(1));

        // Advance 20min into 1h period, then trigger with Replace.
        // Remaining is ~40min. Next execution at remaining + period = ~40min + 1h = ~100min from now.
        timeProvider.Advance(TimeSpan.FromMinutes(20));
        sut.PublicTriggerExecutionReplace();
        Assert.That(await executed.WaitAsync(TimeSpan.FromSeconds(5)), Is.True, "Triggered execution should complete");
        Assert.That(executionCount, Is.EqualTo(2));

        // The original next tick at 40min should be skipped. Advance to 60min — past the skipped tick.
        timeProvider.Advance(TimeSpan.FromMinutes(60));
        Assert.That(await executed.WaitAsync(TimeSpan.FromMilliseconds(50)), Is.False, "Should not execute — skipped scheduled tick");

        // Advance to the tick after the skipped one (~100min from trigger, ~40min more)
        timeProvider.Advance(TimeSpan.FromMinutes(40));
        Assert.That(await executed.WaitAsync(TimeSpan.FromSeconds(5)), Is.True);
        Assert.That(executionCount, Is.EqualTo(3), "Should execute at tick after the skipped one");

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

        Assert.That(await executed.WaitAsync(TimeSpan.FromSeconds(5)), Is.True);
        Assert.That(executionCount, Is.EqualTo(1));

        // Trigger with a custom 10-minute delay
        sut.PublicTriggerExecutionWithDelay(TimeSpan.FromMinutes(10));
        Assert.That(await executed.WaitAsync(TimeSpan.FromSeconds(5)), Is.True, "Triggered execution should complete");
        Assert.That(executionCount, Is.EqualTo(2));

        // After the triggered execution, next should come after the custom 10min delay
        timeProvider.Advance(TimeSpan.FromMinutes(9));
        Assert.That(await executed.WaitAsync(TimeSpan.FromMilliseconds(50)), Is.False, "Should not execute before custom delay");

        timeProvider.Advance(TimeSpan.FromMinutes(1));
        Assert.That(await executed.WaitAsync(TimeSpan.FromSeconds(5)), Is.True);
        Assert.That(executionCount, Is.EqualTo(3), "Should execute after custom delay");

        cts.Cancel();
        await sut.StopAsync(CancellationToken.None);
    }

    [Test]
    public async Task TriggerExecution_During_InitialDelay_Honors_Custom_Delay()
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
        Assert.That(await executed.WaitAsync(TimeSpan.FromMilliseconds(50)), Is.False, "Should not have executed during initial delay");

        // Trigger with a custom 5-minute next-delay during the initial delay
        sut.PublicTriggerExecutionWithDelay(TimeSpan.FromMinutes(5));
        Assert.That(await executed.WaitAsync(TimeSpan.FromSeconds(5)), Is.True);
        Assert.That(executionCount, Is.EqualTo(1), "Should have executed once after trigger interrupted delay");

        // The custom 5-minute delay applies to the next wait — consistent with TriggerExecution(TimeSpan) docs.
        timeProvider.Advance(TimeSpan.FromMinutes(4));
        Assert.That(await executed.WaitAsync(TimeSpan.FromMilliseconds(50)), Is.False, "Should not execute before the custom next-delay elapses");

        timeProvider.Advance(TimeSpan.FromMinutes(1));
        Assert.That(await executed.WaitAsync(TimeSpan.FromSeconds(5)), Is.True);
        Assert.That(executionCount, Is.EqualTo(2), "Second execution fires after the custom 5-minute delay");

        // After the trigger's custom delay is consumed, the loop resumes the normal 1-hour period.
        timeProvider.Advance(TimeSpan.FromMinutes(59));
        Assert.That(await executed.WaitAsync(TimeSpan.FromMilliseconds(50)), Is.False, "Should not execute before the normal period elapses");

        timeProvider.Advance(TimeSpan.FromMinutes(1));
        Assert.That(await executed.WaitAsync(TimeSpan.FromSeconds(5)), Is.True);
        Assert.That(executionCount, Is.EqualTo(3), "Third execution fires after the normal 1-hour period");

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
                if (count == 1)
                {
                    // Advance past the period so the next execution fires immediately
                    // after the loop catches the exception (no wait needed).
                    timeProvider.Advance(TimeSpan.FromMinutes(5));
                    throw new InvalidOperationException("Test exception");
                }

                executed.Release();
                return Task.CompletedTask;
            });

        using var cts = new CancellationTokenSource();
        await sut.StartAsync(cts.Token);

        // The first execution throws (after advancing time), so the loop immediately retries.
        // The second execution succeeds and signals the semaphore.
        Assert.That(await executed.WaitAsync(TimeSpan.FromSeconds(5)), Is.True, "Second execution should complete despite first throwing");
        Assert.That(executionCount, Is.EqualTo(2), "Loop should continue after exception");

        cts.Cancel();
        await sut.StopAsync(CancellationToken.None);
    }

    [Test]
    public async Task ChangePeriod_Takes_Effect_Immediately()
    {
        var executionCount = 0;
        var timeProvider = new FakeTimeProvider();
        using var executed = new SemaphoreSlim(0);
        var sut = new TestRecurringHostedService(
            period: TimeSpan.FromMinutes(10),
            delay: TimeSpan.Zero,
            timeProvider: timeProvider,
            onExecute: _ => { Interlocked.Increment(ref executionCount); executed.Release(); return Task.CompletedTask; });

        using var cts = new CancellationTokenSource();
        await sut.StartAsync(cts.Token);

        Assert.That(await executed.WaitAsync(TimeSpan.FromSeconds(5)), Is.True);
        Assert.That(executionCount, Is.EqualTo(1));

        // Change to a 1-hour period — should interrupt the in-flight wait immediately.
        sut.PublicChangePeriod(TimeSpan.FromHours(1));

        // Advancing the old 10min period should NOT trigger execution.
        timeProvider.Advance(TimeSpan.FromMinutes(10));
        Assert.That(executionCount, Is.EqualTo(1), "Should not execute at old period interval");

        // Advancing to 1h total from first execution should trigger.
        timeProvider.Advance(TimeSpan.FromMinutes(50));
        Assert.That(await executed.WaitAsync(TimeSpan.FromSeconds(5)), Is.True);
        Assert.That(executionCount, Is.EqualTo(2), "Should execute after new period");

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
