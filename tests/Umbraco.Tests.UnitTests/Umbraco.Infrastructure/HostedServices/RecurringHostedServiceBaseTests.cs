// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Infrastructure.HostedServices;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.HostedServices;

[TestFixture]
public class RecurringHostedServiceBaseTests
{
    [TestCase("30 12 * * *", 30)]
    [TestCase("15 18 * * *", (60 * 6) + 15)]
    [TestCase("0 3 * * *", 60 * 15)]
    [TestCase("0 3 2 * *", (24 * 60 * 1) + (60 * 15))]
    [TestCase("0 6 * * 3", (24 * 60 * 3) + (60 * 18))]
    public void Returns_Notification_Delay_From_Provided_Time(string firstRunTime, int expectedDelayInMinutes)
    {
        var cronTabParser = new NCronTabParser();
        var logger = Mock.Of<ILogger>();
        var now = new DateTime(2020, 10, 31, 12, 0, 0);
        var result = RecurringHostedServiceBase.GetDelay(firstRunTime, cronTabParser, logger, now, TimeSpan.Zero);
        Assert.AreEqual(expectedDelayInMinutes, result.TotalMinutes);
    }

    [Test]
    public void Returns_Notification_Delay_From_Default_When_Provided_Time_Too_Close_To_Current_Time()
    {
        var firstRunTime = "30 12 * * *";
        var cronTabParser = new NCronTabParser();
        var logger = Mock.Of<ILogger>();
        var now = new DateTime(2020, 10, 31, 12, 25, 0);
        var defaultDelay = TimeSpan.FromMinutes(10);
        var result = RecurringHostedServiceBase.GetDelay(firstRunTime, cronTabParser, logger, now, defaultDelay);
        Assert.AreEqual(defaultDelay.TotalMinutes, result.TotalMinutes);
    }

    [Test]
    public void Logs_And_Returns_Notification_Delay_From_Default_When_Provided_Time_Is_Not_Valid()
    {
        var firstRunTime = "invalid";
        var cronTabParser = new NCronTabParser();
        var logger = new Mock<ILogger>();
        var now = new DateTime(2020, 10, 31, 12, 25, 0);
        var defaultDelay = TimeSpan.FromMinutes(10);
        var result = RecurringHostedServiceBase.GetDelay(firstRunTime, cronTabParser, logger.Object, now, defaultDelay);
        Assert.AreEqual(defaultDelay, result);

        logger.Verify(
            logger => logger.Log(
                It.Is<LogLevel>(y => y == LogLevel.Warning),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Test]
    public void ComputeNextDelay_Subtracts_Elapsed_Time_From_Period()
    {
        var period = TimeSpan.FromSeconds(10);
        var elapsed = TimeSpan.FromSeconds(3);

        TimeSpan result = RecurringHostedServiceBase.ComputeNextDelay(period, elapsed);

        Assert.AreEqual(TimeSpan.FromSeconds(7), result);
    }

    [Test]
    public void ComputeNextDelay_Returns_Zero_When_Execution_Exceeds_Period()
    {
        var period = TimeSpan.FromSeconds(10);
        var elapsed = TimeSpan.FromSeconds(15);

        TimeSpan result = RecurringHostedServiceBase.ComputeNextDelay(period, elapsed);

        Assert.AreEqual(TimeSpan.Zero, result);
    }

    [Test]
    public void ComputeNextDelay_Returns_Full_Period_When_Elapsed_Is_Zero()
    {
        var period = TimeSpan.FromSeconds(10);
        var elapsed = TimeSpan.Zero;

        TimeSpan result = RecurringHostedServiceBase.ComputeNextDelay(period, elapsed);

        Assert.AreEqual(period, result);
    }

    [Test]
    public void ComputeNextDelay_Returns_Zero_When_Execution_Equals_Period()
    {
        var period = TimeSpan.FromSeconds(10);
        var elapsed = TimeSpan.FromSeconds(10);

        TimeSpan result = RecurringHostedServiceBase.ComputeNextDelay(period, elapsed);

        Assert.AreEqual(TimeSpan.Zero, result);
    }

    [Test]
    public void ComputeNextDelay_Returns_Zero_For_Negative_Period()
    {
        var period = TimeSpan.FromMilliseconds(-1);
        var elapsed = TimeSpan.FromSeconds(1);

        TimeSpan result = RecurringHostedServiceBase.ComputeNextDelay(period, elapsed);

        Assert.AreEqual(TimeSpan.Zero, result);
    }

    [Test]
    public async Task Loop_Executes_Periodically_And_Respects_Cancellation()
    {
        var executionCount = 0;
        var sut = new TestRecurringHostedService(
            period: TimeSpan.FromMilliseconds(50),
            delay: TimeSpan.Zero,
            onExecute: _ => { Interlocked.Increment(ref executionCount); return Task.CompletedTask; });

        using var cts = new CancellationTokenSource();
        await sut.StartAsync(cts.Token);

        // Wait long enough for multiple executions
        await Task.Delay(300);
        cts.Cancel();
        await sut.StopAsync(CancellationToken.None);

        Assert.GreaterOrEqual(executionCount, 2, "Should have executed multiple times");
    }

    [Test]
    public async Task TriggerExecution_Causes_Immediate_Execution()
    {
        var executionCount = 0;
        var sut = new TestRecurringHostedService(
            period: TimeSpan.FromSeconds(30),
            delay: TimeSpan.Zero,
            onExecute: _ => { Interlocked.Increment(ref executionCount); return Task.CompletedTask; });

        using var cts = new CancellationTokenSource();
        await sut.StartAsync(cts.Token);

        // Wait for first execution
        await Task.Delay(100);
        var countAfterFirst = executionCount;
        Assert.AreEqual(1, countAfterFirst, "Should have executed once initially");

        // Trigger early execution (period is 30s, so without trigger we wouldn't get another)
        sut.PublicTriggerExecution();
        await Task.Delay(100);

        Assert.AreEqual(2, executionCount, "Should have executed again after trigger");

        cts.Cancel();
        await sut.StopAsync(CancellationToken.None);
    }

    [Test]
    public async Task TriggerExecution_Reset_Starts_New_Full_Period()
        => await AssertTriggerExecutionBehavior(
            sut => sut.PublicTriggerExecutionReset(),
            expectedCountAfterTrigger: 2,
            afterTriggerMessage: "Should have executed again after trigger",
            expectedCountAfterWait: 2,
            afterWaitMessage: "Should not execute again within 30s period");

    [Test]
    public async Task TriggerExecution_None_Resumes_Original_Wait()
        => await AssertTriggerExecutionBehavior(
            sut => sut.PublicTriggerExecutionNone(),
            expectedCountAfterTrigger: 2,
            afterTriggerMessage: "Should have executed again after trigger",
            expectedCountAfterWait: 2,
            afterWaitMessage: "Should not execute again — remaining schedule still active");

    [Test]
    public async Task TriggerExecution_None_Skips_Overshot_Execution()
    {
        var executionCount = 0;
        var sut = new TestRecurringHostedService(
            period: TimeSpan.FromMilliseconds(500),
            delay: TimeSpan.Zero,
            onExecute: async _ =>
            {
                var count = Interlocked.Increment(ref executionCount);
                if (count == 2)
                {
                    // Triggered execution takes longer than the remaining time to the next tick
                    await Task.Delay(600);
                }
            });

        using var cts = new CancellationTokenSource();
        await sut.StartAsync(cts.Token);

        // Wait for first execution
        await Task.Delay(50);
        Assert.AreEqual(1, executionCount, "Should have executed once initially");

        // Trigger with None. Remaining is ~450ms, but the triggered execution takes 600ms.
        // The scheduled tick at ~500ms passes during execution. It should NOT execute immediately.
        sut.PublicTriggerExecutionNone();

        // Triggered execution finishes at ~650ms from start. The overshoot is ~100ms.
        // Next period tick is at ~500ms + 500ms = ~1000ms from start, so ~350ms from now.
        // Verify no immediate execution after the triggered one finishes.
        await Task.Delay(750);
        Assert.AreEqual(2, executionCount, "Should not have executed again — scheduled tick was overshot and skipped");

        // Wait for the next period tick
        await Task.Delay(400);
        Assert.AreEqual(3, executionCount, "Should execute at next period tick");

        cts.Cancel();
        await sut.StopAsync(CancellationToken.None);
    }

    [Test]
    public async Task TriggerExecution_Replace_Skips_Next_Tick()
    {
        var executionCount = 0;
        var sut = new TestRecurringHostedService(
            period: TimeSpan.FromMilliseconds(200),
            delay: TimeSpan.Zero,
            onExecute: _ => { Interlocked.Increment(ref executionCount); return Task.CompletedTask; });

        using var cts = new CancellationTokenSource();
        await sut.StartAsync(cts.Token);

        // Wait for first execution
        await Task.Delay(50);
        Assert.AreEqual(1, executionCount, "Should have executed once initially");

        // Trigger with Replace — the triggered execution replaces the next tick.
        // Next execution should be at ~original_tick + period = ~200ms + 200ms = ~400ms from start.
        sut.PublicTriggerExecutionReplace();
        await Task.Delay(50);
        Assert.AreEqual(2, executionCount, "Should have executed after trigger");

        // At ~100ms from start. The next tick is ~400ms from start, so ~300ms from now.
        // No execution should happen in the next 200ms.
        await Task.Delay(200);
        Assert.AreEqual(2, executionCount, "Should not execute — skipped scheduled tick");

        // Wait enough for the tick after the skipped one (~200ms more)
        await Task.Delay(250);
        Assert.AreEqual(3, executionCount, "Should execute at tick after the skipped one");

        cts.Cancel();
        await sut.StopAsync(CancellationToken.None);
    }

    [Test]
    public async Task TriggerExecution_CustomDelay_Uses_Specified_Delay()
        => await AssertTriggerExecutionBehavior(
            sut => sut.PublicTriggerExecutionWithDelay(TimeSpan.FromMilliseconds(50)),
            expectedCountAfterTrigger: 3,
            afterTriggerMessage: "Should have executed: initial + trigger + after custom delay",
            expectedCountAfterWait: 3,
            afterWaitMessage: "Should not execute again within 30s period");

    [Test]
    public async Task TriggerExecution_During_InitialDelay_Does_Not_Leak_Strategy()
    {
        var executionCount = 0;
        var sut = new TestRecurringHostedService(
            period: TimeSpan.FromSeconds(30),
            delay: TimeSpan.FromSeconds(30),
            onExecute: _ => { Interlocked.Increment(ref executionCount); return Task.CompletedTask; });

        using var cts = new CancellationTokenSource();
        await sut.StartAsync(cts.Token);

        // No execution yet — still in initial delay
        await Task.Delay(50);
        Assert.AreEqual(0, executionCount, "Should not have executed during initial delay");

        // Trigger with a custom delay during the initial delay
        sut.PublicTriggerExecutionWithDelay(TimeSpan.FromMilliseconds(50));
        await Task.Delay(100);
        Assert.AreEqual(1, executionCount, "Should have executed once after trigger interrupted delay");

        // The custom delay should NOT be applied after the first execution —
        // the strategy was consumed when the initial delay was interrupted.
        // The next wait should use the normal 30s period, so no more executions.
        await Task.Delay(200);
        Assert.AreEqual(1, executionCount, "Custom delay should not leak — next wait uses normal period");

        cts.Cancel();
        await sut.StopAsync(CancellationToken.None);
    }

    [Test]
    public async Task Exception_In_PerformExecuteAsync_Does_Not_Kill_Loop()
    {
        var executionCount = 0;
        var sut = new TestRecurringHostedService(
            period: TimeSpan.FromMilliseconds(50),
            delay: TimeSpan.Zero,
            onExecute: _ =>
            {
                Interlocked.Increment(ref executionCount);
                if (executionCount == 1)
                {
                    throw new InvalidOperationException("Test exception");
                }
                return Task.CompletedTask;
            });

        using var cts = new CancellationTokenSource();
        await sut.StartAsync(cts.Token);

        await Task.Delay(300);
        cts.Cancel();
        await sut.StopAsync(CancellationToken.None);

        Assert.GreaterOrEqual(executionCount, 2, "Loop should continue after exception");
    }

    [Test]
    public async Task ChangePeriod_Applies_New_Period_On_Next_Cycle()
    {
        var executionCount = 0;
        var sut = new TestRecurringHostedService(
            period: TimeSpan.FromMilliseconds(100),
            delay: TimeSpan.Zero,
            onExecute: _ => { Interlocked.Increment(ref executionCount); return Task.CompletedTask; });

        using var cts = new CancellationTokenSource();
        await sut.StartAsync(cts.Token);

        // Wait for first execution + one period tick
        await Task.Delay(250);
        var countBeforeChange = executionCount;
        Assert.GreaterOrEqual(countBeforeChange, 2, "Should have executed at least twice with short period");

        // Change to a long period — subsequent executions should stop
        sut.PublicChangePeriod(TimeSpan.FromSeconds(30));

        // Allow one in-flight execution to complete, then snapshot
        await Task.Delay(150);
        var countAfterChange = executionCount;

        // With a 30s period, no further executions should happen in the next 300ms
        await Task.Delay(300);
        Assert.AreEqual(countAfterChange, executionCount, "No additional executions should occur with long period");

        cts.Cancel();
        await sut.StopAsync(CancellationToken.None);
    }

    private async Task AssertTriggerExecutionBehavior(
        Action<TestRecurringHostedService> trigger,
        int expectedCountAfterTrigger,
        string afterTriggerMessage,
        int expectedCountAfterWait,
        string afterWaitMessage)
    {
        var executionCount = 0;
        var sut = new TestRecurringHostedService(
            period: TimeSpan.FromSeconds(30),
            delay: TimeSpan.Zero,
            onExecute: _ => { Interlocked.Increment(ref executionCount); return Task.CompletedTask; });

        using var cts = new CancellationTokenSource();
        await sut.StartAsync(cts.Token);

        await Task.Delay(100);
        Assert.AreEqual(1, executionCount);

        trigger(sut);
        await Task.Delay(200);
        Assert.AreEqual(expectedCountAfterTrigger, executionCount, afterTriggerMessage);

        await Task.Delay(300);
        Assert.AreEqual(expectedCountAfterWait, executionCount, afterWaitMessage);

        cts.Cancel();
        await sut.StopAsync(CancellationToken.None);
    }

    /// <summary>
    /// A concrete test subclass that overrides the new PerformExecuteAsync(CancellationToken).
    /// </summary>
    private class TestRecurringHostedService : RecurringHostedServiceBase
    {
        private readonly Func<CancellationToken, Task> _onExecute;

        public TestRecurringHostedService(TimeSpan period, TimeSpan delay, Func<CancellationToken, Task> onExecute)
            : base(null, period, delay)
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
