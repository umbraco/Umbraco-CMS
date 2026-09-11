// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Infrastructure.BackgroundJobs;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.BackgroundJobs;

[TestFixture]
public class RecurringBackgroundJobBaseTests
{
    [Test]
    public void Constructor_Sets_Initial_Period()
    {
        var sut = new TestJob(TimeSpan.FromMinutes(7));

        Assert.That(sut.Period, Is.EqualTo(TimeSpan.FromMinutes(7)));
    }

    [Test]
    public void Constructor_Allows_Infinite_Period()
    {
        var sut = new TestJob(Timeout.InfiniteTimeSpan);

        Assert.That(sut.Period, Is.EqualTo(Timeout.InfiniteTimeSpan));
    }

    [Test]
    public void Constructor_Throws_On_Negative_Period()
        => Assert.Throws<ArgumentOutOfRangeException>(() => new TestJob(TimeSpan.FromSeconds(-1)));

    [Test]
    public void IgnoredDelay_Default_Is_DefaultIgnoredDelay()
    {
        var sut = new TestJob(Timeout.InfiniteTimeSpan);

        Assert.That(sut.IgnoredDelay, Is.EqualTo(RecurringBackgroundJobBase.DefaultIgnoredDelay));
    }

    [Test]
    public void Setting_Period_Raises_PeriodChanged()
    {
        var sut = new TestJob(Timeout.InfiniteTimeSpan);
        var raised = 0;
        sut.PeriodChanged += (_, _) => raised++;

        sut.SetPeriod(TimeSpan.FromMinutes(5));

        Assert.That(sut.Period, Is.EqualTo(TimeSpan.FromMinutes(5)));
        Assert.That(raised, Is.EqualTo(1));
    }

    [Test]
    public void Setting_Period_To_Same_Value_Does_Not_Raise()
    {
        var sut = new TestJob(TimeSpan.FromMinutes(5));
        var raised = 0;
        sut.PeriodChanged += (_, _) => raised++;

        sut.SetPeriod(TimeSpan.FromMinutes(5));

        Assert.That(raised, Is.EqualTo(0));
    }

    [Test]
    public void Setting_Period_To_Infinite_Is_Allowed()
    {
        var sut = new TestJob(TimeSpan.FromMinutes(5));
        var raised = 0;
        sut.PeriodChanged += (_, _) => raised++;

        sut.SetPeriod(Timeout.InfiniteTimeSpan);

        Assert.That(sut.Period, Is.EqualTo(Timeout.InfiniteTimeSpan));
        Assert.That(raised, Is.EqualTo(1));
    }

    [Test]
    public void Setting_Period_To_Negative_Throws()
    {
        var sut = new TestJob(Timeout.InfiniteTimeSpan);

        Assert.Throws<ArgumentOutOfRangeException>(() => sut.SetPeriod(TimeSpan.FromSeconds(-1)));
    }

    [Test]
    public void Setting_IgnoredDelay_Raises_IgnoredDelayChanged()
    {
        var sut = new TestJob(Timeout.InfiniteTimeSpan);
        var raised = 0;
        sut.IgnoredDelayChanged += (_, _) => raised++;

        sut.SetIgnoredDelay(TimeSpan.FromSeconds(30));

        Assert.That(sut.IgnoredDelay, Is.EqualTo(TimeSpan.FromSeconds(30)));
        Assert.That(raised, Is.EqualTo(1));
    }

    [Test]
    public void Setting_IgnoredDelay_To_Same_Value_Does_Not_Raise()
    {
        var sut = new TestJob(Timeout.InfiniteTimeSpan);
        sut.SetIgnoredDelay(TimeSpan.FromSeconds(30));
        var raised = 0;
        sut.IgnoredDelayChanged += (_, _) => raised++;

        sut.SetIgnoredDelay(TimeSpan.FromSeconds(30));

        Assert.That(raised, Is.EqualTo(0));
    }

    [Test]
    public void Setting_IgnoredDelay_To_Infinite_Is_Allowed()
    {
        var sut = new TestJob(Timeout.InfiniteTimeSpan);
        var raised = 0;
        sut.IgnoredDelayChanged += (_, _) => raised++;

        sut.SetIgnoredDelay(Timeout.InfiniteTimeSpan);

        Assert.That(sut.IgnoredDelay, Is.EqualTo(Timeout.InfiniteTimeSpan));
        Assert.That(raised, Is.EqualTo(1));
    }

    [Test]
    public void Setting_IgnoredDelay_To_Negative_Throws()
    {
        var sut = new TestJob(Timeout.InfiniteTimeSpan);

        Assert.Throws<ArgumentOutOfRangeException>(() => sut.SetIgnoredDelay(TimeSpan.FromSeconds(-1)));
    }

    [Test]
    public void Dispose_Clears_PeriodChanged_Subscribers()
    {
        var sut = new TestJob(TimeSpan.FromMinutes(5));
        var raised = 0;
        sut.PeriodChanged += (_, _) => raised++;

        sut.Dispose();
        sut.SetPeriod(TimeSpan.FromMinutes(6));

        Assert.That(raised, Is.EqualTo(0));
    }

    [Test]
    public void Dispose_Clears_IgnoredDelayChanged_Subscribers()
    {
        var sut = new TestJob(Timeout.InfiniteTimeSpan);
        var raised = 0;
        sut.IgnoredDelayChanged += (_, _) => raised++;

        sut.Dispose();
        sut.SetIgnoredDelay(TimeSpan.FromSeconds(30));

        Assert.That(raised, Is.EqualTo(0));
    }

    private sealed class TestJob : RecurringBackgroundJobBase
    {
        public TestJob(TimeSpan period)
            : base(period)
        {
        }

        public override Task RunJobAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public void SetPeriod(TimeSpan value) => Period = value;

        public void SetIgnoredDelay(TimeSpan value) => IgnoredDelay = value;
    }
}
