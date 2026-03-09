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
    public async Task ChangePeriod_Wakes_Loop_With_New_Period()
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
        Assert.AreEqual(1, executionCount);

        // Change period to something short — this should also trigger immediate execution
        sut.PublicChangePeriod(TimeSpan.FromMilliseconds(50));
        await Task.Delay(200);

        Assert.GreaterOrEqual(executionCount, 2, "ChangePeriod should wake the loop");

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

        public void PublicChangePeriod(TimeSpan newPeriod) => ChangePeriod(newPeriod);
    }
}
