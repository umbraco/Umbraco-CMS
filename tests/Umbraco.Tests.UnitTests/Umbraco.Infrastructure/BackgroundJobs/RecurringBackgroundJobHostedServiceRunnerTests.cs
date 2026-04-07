// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Infrastructure.BackgroundJobs;
using Umbraco.Cms.Infrastructure.HostedServices;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.BackgroundJobs;

[TestFixture]
public class RecurringBackgroundJobHostedServiceRunnerTests
{
    [Test]
    public async Task TriggerExecution_Returns_True_When_Job_Is_Running()
    {
        var sut = CreateRunner(new TestJobA());
        await sut.StartAsync(CancellationToken.None);

        bool result = sut.TriggerExecution<TestJobA>();

        Assert.IsTrue(result);

        await StopAsync(sut);
    }

    [Test]
    public async Task TriggerExecution_Returns_False_When_Job_Is_Not_Registered()
    {
        var sut = CreateRunner(new TestJobA());
        await sut.StartAsync(CancellationToken.None);

        bool result = sut.TriggerExecution<TestJobB>();

        Assert.IsFalse(result);

        await StopAsync(sut);
    }

    [Test]
    public async Task TriggerExecution_Returns_False_Before_StartAsync()
    {
        var sut = CreateRunner(new TestJobA());

        bool result = sut.TriggerExecution<TestJobA>();

        Assert.IsFalse(result);
    }

    [Test]
    public async Task TriggerExecution_With_Strategy_Returns_True_When_Job_Is_Running()
    {
        var sut = CreateRunner(new TestJobA());
        await sut.StartAsync(CancellationToken.None);

        bool result = sut.TriggerExecution<TestJobA>(NextExecutionStrategy.Reset);

        Assert.IsTrue(result);

        await StopAsync(sut);
    }

    [Test]
    public async Task TriggerExecution_With_Delay_Returns_True_When_Job_Is_Running()
    {
        var sut = CreateRunner(new TestJobA());
        await sut.StartAsync(CancellationToken.None);

        bool result = sut.TriggerExecution<TestJobA>(TimeSpan.FromSeconds(10));

        Assert.IsTrue(result);

        await StopAsync(sut);
    }

    [Test]
    public async Task TriggerExecution_Causes_Immediate_Execution()
    {
        var executionCount = 0;
        using var executed = new SemaphoreSlim(0);
        var job = new TestJobA(onExecute: _ => { Interlocked.Increment(ref executionCount); executed.Release(); return Task.CompletedTask; });

        var sut = CreateRunner(job);
        await sut.StartAsync(CancellationToken.None);

        // Wait for first execution (no delay on TestJobA)
        Assert.IsTrue(await executed.WaitAsync(TimeSpan.FromSeconds(5)), "First execution should complete");
        Assert.AreEqual(1, executionCount, "Should have executed once initially");

        // Trigger — period is 30s, so without trigger we wouldn't get another
        sut.TriggerExecution<TestJobA>();
        Assert.IsTrue(await executed.WaitAsync(TimeSpan.FromSeconds(5)), "Triggered execution should complete");
        Assert.AreEqual(2, executionCount, "Should have executed again after trigger");

        await StopAsync(sut);
    }

    private static RecurringBackgroundJobHostedServiceRunner CreateRunner(params IRecurringBackgroundJob[] jobs)
    {
        var logger = Mock.Of<ILogger<RecurringBackgroundJobHostedServiceRunner>>();
        Func<IRecurringBackgroundJob, IHostedService> factory = job =>
            new TestHostedService(job.Period, job.Delay, job, TimeProvider.System);

        return new RecurringBackgroundJobHostedServiceRunner(logger, jobs, factory);
    }

    private static async Task StopAsync(RecurringBackgroundJobHostedServiceRunner runner)
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        await runner.StopAsync(cts.Token);
    }

    private class TestJobA : RecurringBackgroundJobBase
    {
        private readonly Func<CancellationToken, Task>? _onExecute;

        public TestJobA(Func<CancellationToken, Task>? onExecute = null) => _onExecute = onExecute;

        public override TimeSpan Period => TimeSpan.FromSeconds(30);

        public override TimeSpan Delay => TimeSpan.Zero;

        public override Task RunJobAsync(CancellationToken cancellationToken)
            => _onExecute?.Invoke(cancellationToken) ?? Task.CompletedTask;
    }

    private class TestJobB : RecurringBackgroundJobBase
    {
        public override TimeSpan Period => TimeSpan.FromSeconds(30);

        public override TimeSpan Delay => TimeSpan.Zero;

        public override Task RunJobAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }

    /// <summary>
    /// Minimal hosted service that wraps a job, inheriting from RecurringHostedServiceBase
    /// so the runner can cast and call TriggerExecution.
    /// </summary>
    private class TestHostedService : RecurringHostedServiceBase
    {
        private readonly IRecurringBackgroundJob _job;

        public TestHostedService(TimeSpan period, TimeSpan delay, IRecurringBackgroundJob job, TimeProvider timeProvider)
            : base(null, period, delay, timeProvider)
            => _job = job;

        public override Task PerformExecuteAsync(CancellationToken stoppingToken)
            => _job.RunJobAsync(stoppingToken);
    }
}
