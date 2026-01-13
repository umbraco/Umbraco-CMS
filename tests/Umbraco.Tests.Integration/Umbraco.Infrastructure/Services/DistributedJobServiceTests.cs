using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Infrastructure.BackgroundJobs;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Services;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

/// <summary>
/// Integration tests for <see cref="IDistributedJobService"/>.
/// </summary>
[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class DistributedJobServiceTests : UmbracoIntegrationTest
{
    private const string TestJobName = "TestDistributedJob";
    private static readonly TimeSpan TestJobPeriod = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan TestMaxExecutionTime = TimeSpan.FromMinutes(10);

    private IDistributedJobService DistributedJobService => GetRequiredService<IDistributedJobService>();

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        // Register a test job
        builder.Services.AddSingleton<IDistributedBackgroundJob, TestDistributedJob>();

        // Configure settings with a known MaximumExecutionTime for testing
        builder.Services.PostConfigure<DistributedJobSettings>(options =>
        {
            options.MaximumExecutionTime = TestMaxExecutionTime;
        });
    }

    [Test]
    public async Task TryTakeRunnableAsync_JobIsDueAndNotRunning_ReturnsJob()
    {
        // Arrange - Ensure jobs are registered
        await DistributedJobService.EnsureJobsAsync();

        // Set the job's LastRun to be older than the period
        SetJobState(TestJobName, lastRun: DateTime.UtcNow - TestJobPeriod - TimeSpan.FromMinutes(1), isRunning: false);

        // Act
        var job = await DistributedJobService.TryTakeRunnableAsync();

        // Assert
        Assert.IsNotNull(job);
        Assert.AreEqual(TestJobName, job!.Name);

        // Verify the job is now marked as running
        var jobState = GetJobState(TestJobName);
        Assert.IsTrue(jobState.IsRunning);
    }

    [Test]
    public async Task TryTakeRunnableAsync_JobIsNotDue_ReturnsNull()
    {
        // Arrange - Ensure jobs are registered
        await DistributedJobService.EnsureJobsAsync();

        // Set the job's LastRun to be recent (not due yet)
        SetJobState(TestJobName, lastRun: DateTime.UtcNow, isRunning: false);

        // Act
        var job = await DistributedJobService.TryTakeRunnableAsync();

        // Assert
        Assert.IsNull(job);
    }

    [Test]
    public async Task TryTakeRunnableAsync_JobIsRunningAndNotTimedOut_ReturnsNull()
    {
        // Arrange - Ensure jobs are registered
        await DistributedJobService.EnsureJobsAsync();

        // Set the job as running with a recent LastAttemptedRun (not timed out)
        SetJobState(
            TestJobName,
            lastRun: DateTime.UtcNow - TestJobPeriod - TimeSpan.FromMinutes(1),
            isRunning: true,
            lastAttemptedRun: DateTime.UtcNow);

        // Act
        var job = await DistributedJobService.TryTakeRunnableAsync();

        // Assert - Should NOT pick up the job because it's running and hasn't timed out
        Assert.IsNull(job);
    }

    [Test]
    public async Task TryTakeRunnableAsync_JobIsRunningButTimedOut_ReturnsJob()
    {
        // Arrange - Ensure jobs are registered
        await DistributedJobService.EnsureJobsAsync();

        // Set the job as running but with LastAttemptedRun older than Period + MaxExecutionTime (timed out)
        var timedOutTime = DateTime.UtcNow - TestJobPeriod - TestMaxExecutionTime - TimeSpan.FromMinutes(1);
        SetJobState(
            TestJobName,
            lastRun: timedOutTime - TimeSpan.FromMinutes(1),
            isRunning: true,
            lastAttemptedRun: timedOutTime);

        // Act
        var job = await DistributedJobService.TryTakeRunnableAsync();

        // Assert - Should pick up the job because it has timed out (stale recovery)
        Assert.IsNotNull(job);
        Assert.AreEqual(TestJobName, job!.Name);
    }

    [Test]
    public async Task FinishAsync_MarksJobAsNotRunningAndUpdatesLastRun()
    {
        // Arrange - Ensure jobs are registered and take a job
        await DistributedJobService.EnsureJobsAsync();
        SetJobState(TestJobName, lastRun: DateTime.UtcNow - TestJobPeriod - TimeSpan.FromMinutes(1), isRunning: false);

        var job = await DistributedJobService.TryTakeRunnableAsync();
        Assert.IsNotNull(job);

        var beforeFinish = DateTime.UtcNow;

        // Act
        await DistributedJobService.FinishAsync(job!.Name);

        // Assert
        var jobState = GetJobState(TestJobName);
        Assert.IsFalse(jobState.IsRunning);
        Assert.GreaterOrEqual(jobState.LastRun, beforeFinish);
        Assert.GreaterOrEqual(jobState.LastAttemptedRun, beforeFinish);
    }

    [Test]
    public async Task EnsureJobsAsync_RegistersNewJobs()
    {
        // Act
        await DistributedJobService.EnsureJobsAsync();

        // Assert
        var jobState = GetJobState(TestJobName);
        Assert.IsNotNull(jobState);
        Assert.AreEqual(TestJobPeriod.Ticks, jobState.Period);
    }

    [Test]
    public async Task TryTakeRunnableAsync_MultipleJobsDue_ReturnsFirstDueJob()
    {
        // Arrange - Ensure jobs are registered
        await DistributedJobService.EnsureJobsAsync();

        // Set the job's LastRun to be older than the period
        SetJobState(TestJobName, lastRun: DateTime.UtcNow - TestJobPeriod - TimeSpan.FromMinutes(1), isRunning: false);

        // Act - Take the first job
        var job1 = await DistributedJobService.TryTakeRunnableAsync();
        Assert.IsNotNull(job1);

        // Act - Try to take another job (same job is now running, so should return null if no other jobs are due)
        var job2 = await DistributedJobService.TryTakeRunnableAsync();

        // Assert - Should be null because the only test job is now running
        Assert.IsNull(job2);
    }

    [Test]
    public async Task TryTakeRunnableAsync_JobRunningJustBeforeTimeout_ReturnsNull()
    {
        // Arrange - Ensure jobs are registered
        await DistributedJobService.EnsureJobsAsync();

        // Set the job as running with LastAttemptedRun just slightly before the timeout threshold
        // This tests the boundary condition
        var justBeforeTimeout = DateTime.UtcNow - TestJobPeriod - TestMaxExecutionTime + TimeSpan.FromSeconds(30);
        SetJobState(
            TestJobName,
            lastRun: justBeforeTimeout - TestJobPeriod,
            isRunning: true,
            lastAttemptedRun: justBeforeTimeout);

        // Act
        var job = await DistributedJobService.TryTakeRunnableAsync();

        // Assert - Should NOT pick up because it hasn't quite timed out yet
        Assert.IsNull(job);
    }

    [Test]
    public async Task EnsureJobsAsync_RemovesUnregisteredJobs()
    {
        // Arrange - First ensure our test job is registered
        await DistributedJobService.EnsureJobsAsync();

        // Add an orphaned job directly to the database (simulating a job that was removed from code)
        var orphanedJobName = "OrphanedJob";
        InsertJob(orphanedJobName, TimeSpan.FromHours(1));

        // Verify both jobs exist
        Assert.IsNotNull(GetJobStateOrDefault(TestJobName));
        Assert.IsNotNull(GetJobStateOrDefault(orphanedJobName));

        // Act - EnsureJobsAsync should remove the orphaned job
        await DistributedJobService.EnsureJobsAsync();

        // Assert
        Assert.IsNotNull(GetJobStateOrDefault(TestJobName)); // Registered job still exists
        Assert.IsNull(GetJobStateOrDefault(orphanedJobName)); // Orphaned job was removed
    }

    [Test]
    public async Task EnsureJobsAsync_UpdatesJobPeriodWhenChanged()
    {
        // Arrange - First ensure job is registered
        await DistributedJobService.EnsureJobsAsync();

        // Manually change the period in the database to simulate a mismatch
        var originalPeriod = TestJobPeriod;
        var differentPeriod = TimeSpan.FromHours(99);
        UpdateJobPeriod(TestJobName, differentPeriod);

        // Verify the period was changed
        var beforeUpdate = GetJobState(TestJobName);
        Assert.AreEqual(differentPeriod.Ticks, beforeUpdate.Period);

        // Act - EnsureJobsAsync should update the period back to the registered value
        await DistributedJobService.EnsureJobsAsync();

        // Assert - Period should be restored to the registered value
        var afterUpdate = GetJobState(TestJobName);
        Assert.AreEqual(originalPeriod.Ticks, afterUpdate.Period);
    }

    [Test]
    public async Task EnsureJobsAsync_IsIdempotent()
    {
        // Act - Call EnsureJobsAsync multiple times
        await DistributedJobService.EnsureJobsAsync();
        var afterFirst = GetJobState(TestJobName);

        await DistributedJobService.EnsureJobsAsync();
        var afterSecond = GetJobState(TestJobName);

        await DistributedJobService.EnsureJobsAsync();
        var afterThird = GetJobState(TestJobName);

        // Assert - Job should still exist with same properties
        Assert.AreEqual(afterFirst.Id, afterSecond.Id);
        Assert.AreEqual(afterSecond.Id, afterThird.Id);
        Assert.AreEqual(TestJobPeriod.Ticks, afterThird.Period);
    }

    private void SetJobState(string jobName, DateTime lastRun, bool isRunning, DateTime? lastAttemptedRun = null)
    {
        using var scope = ScopeProvider.CreateScope();
        var sql = ScopeAccessor.AmbientScope!.SqlContext.Sql()
            .Update<DistributedJobDto>(u => u
                .Set(x => x.LastRun, lastRun)
                .Set(x => x.IsRunning, isRunning)
                .Set(x => x.LastAttemptedRun, lastAttemptedRun ?? lastRun))
            .Where<DistributedJobDto>(x => x.Name == jobName);

        ScopeAccessor.AmbientScope.Database.Execute(sql);
        scope.Complete();
    }

    private void InsertJob(string jobName, TimeSpan period)
    {
        using var scope = ScopeProvider.CreateScope();
        var dto = new DistributedJobDto
        {
            Name = jobName,
            Period = period.Ticks,
            LastRun = DateTime.UtcNow,
            IsRunning = false,
            LastAttemptedRun = DateTime.UtcNow,
        };
        ScopeAccessor.AmbientScope!.Database.Insert(dto);
        scope.Complete();
    }

    private void UpdateJobPeriod(string jobName, TimeSpan period)
    {
        using var scope = ScopeProvider.CreateScope();
        var sql = ScopeAccessor.AmbientScope!.SqlContext.Sql()
            .Update<DistributedJobDto>(u => u.Set(x => x.Period, period.Ticks))
            .Where<DistributedJobDto>(x => x.Name == jobName);

        ScopeAccessor.AmbientScope.Database.Execute(sql);
        scope.Complete();
    }

    private DistributedJobDto GetJobState(string jobName)
    {
        using var scope = ScopeProvider.CreateScope();
        var sql = ScopeAccessor.AmbientScope!.SqlContext.Sql()
            .Select<DistributedJobDto>()
            .From<DistributedJobDto>()
            .Where<DistributedJobDto>(x => x.Name == jobName);

        var result = ScopeAccessor.AmbientScope.Database.Single<DistributedJobDto>(sql);
        scope.Complete();
        return result;
    }

    private DistributedJobDto? GetJobStateOrDefault(string jobName)
    {
        using var scope = ScopeProvider.CreateScope();
        var sql = ScopeAccessor.AmbientScope!.SqlContext.Sql()
            .Select<DistributedJobDto>()
            .From<DistributedJobDto>()
            .Where<DistributedJobDto>(x => x.Name == jobName);

        var result = ScopeAccessor.AmbientScope.Database.FirstOrDefault<DistributedJobDto>(sql);
        scope.Complete();
        return result;
    }

    /// <summary>
    /// A simple test implementation of <see cref="IDistributedBackgroundJob"/>.
    /// </summary>
    private sealed class TestDistributedJob : IDistributedBackgroundJob
    {
        public string Name => TestJobName;

        public TimeSpan Period => TestJobPeriod;

        public Task ExecuteAsync() => Task.CompletedTask;
    }
}
