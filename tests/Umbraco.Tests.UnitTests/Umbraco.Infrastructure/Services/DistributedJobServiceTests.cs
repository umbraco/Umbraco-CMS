// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Infrastructure.Models;
using Umbraco.Cms.Infrastructure.Services.Implement;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Services;

/// <summary>
///     Unit tests for <see cref="DistributedJobService" />.
/// </summary>
/// <remarks>
///     Covers <see cref="DistributedJobService.IsDue" />, which decides when a distributed background job is runnable —
///     both the default drift-from-completion behaviour and the opt-in clock-aligned behaviour.
/// </remarks>
[TestFixture]
public class DistributedJobServiceTests
{
    // 2026-01-01 10:00:00 UTC sits exactly on a 10-second clock boundary (anchored at the epoch).
    private static readonly DateTime _onBoundary = new(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc);

    private static DistributedBackgroundJobModel Job(TimeSpan period, DateTime lastRun)
        => new() { Name = "Test", Period = period, LastRun = lastRun };

    [Test]
    public void Can_Run_NonAligned_Job_When_Period_Has_Elapsed_Since_Last_Run()
    {
        DateTime now = _onBoundary;
        var job = Job(TimeSpan.FromMinutes(1), now.AddSeconds(-61));

        Assert.IsTrue(DistributedJobService.IsDue(job, now, aligned: false));
    }

    [Test]
    public void Cannot_Run_NonAligned_Job_Before_Period_Has_Elapsed()
    {
        DateTime now = _onBoundary;
        var job = Job(TimeSpan.FromMinutes(1), now.AddSeconds(-59));

        Assert.IsFalse(DistributedJobService.IsDue(job, now, aligned: false));
    }

    [Test]
    public void Can_Run_Aligned_Job_When_A_Boundary_Has_Passed_Since_Last_Run()
    {
        // now is on the :00 boundary, last run was 5s earlier (within the previous period).
        var job = Job(TimeSpan.FromSeconds(10), _onBoundary.AddSeconds(-5));

        Assert.IsTrue(DistributedJobService.IsDue(job, _onBoundary, aligned: true));
    }

    [Test]
    public void Cannot_Run_Aligned_Job_Between_Boundaries()
    {
        // Ran on the :00 boundary; at :07 the most recent boundary is still :00 -> not due until :10.
        var job = Job(TimeSpan.FromSeconds(10), _onBoundary);

        Assert.IsFalse(DistributedJobService.IsDue(job, _onBoundary.AddSeconds(7), aligned: true));
    }

    [Test]
    public void Can_Run_Aligned_Job_At_The_Next_Boundary()
    {
        var job = Job(TimeSpan.FromSeconds(10), _onBoundary);

        Assert.IsTrue(DistributedJobService.IsDue(job, _onBoundary.AddSeconds(10), aligned: true));
    }

    [Test]
    public void Cannot_Run_Aligned_Job_On_Boundary_Missed_During_Overrun()
    {
        // A run that started on the :00 boundary overran and finished at :15 (period is 10s).
        var job = Job(TimeSpan.FromSeconds(10), _onBoundary.AddSeconds(15));

        // At :18 the most recent boundary (:10) is before the finish (:15) -> not due (no back-to-back catch-up).
        Assert.IsFalse(DistributedJobService.IsDue(job, _onBoundary.AddSeconds(18), aligned: true));
    }

    [Test]
    public void Can_Run_Aligned_Job_At_First_Boundary_After_Overrun()
    {
        // A run that started on the :00 boundary overran and finished at :15 (period is 10s).
        var job = Job(TimeSpan.FromSeconds(10), _onBoundary.AddSeconds(15));

        // At :20 a fresh boundary has passed after the finish -> due.
        Assert.IsTrue(DistributedJobService.IsDue(job, _onBoundary.AddSeconds(20), aligned: true));
    }
}
