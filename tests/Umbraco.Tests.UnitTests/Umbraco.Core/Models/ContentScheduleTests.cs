// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Linq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Models;

[TestFixture]
public class ContentScheduleTests
{
    [Test]
    public void Release_Date_Less_Than_Expire_Date()
    {
        var now = DateTime.UtcNow;
        var schedule = new ContentScheduleCollection();
        Assert.That(schedule.Add(now, now), Is.False);
    }

    [Test]
    public void Cannot_Add_Duplicate_Dates_Invariant()
    {
        var now = DateTime.UtcNow;
        var schedule = new ContentScheduleCollection();
        schedule.Add(now, null);
        Assert.Throws<ArgumentException>(() => schedule.Add(null, now));
    }

    [Test]
    public void Cannot_Add_Duplicate_Dates_Variant()
    {
        var now = DateTime.UtcNow;
        var schedule = new ContentScheduleCollection();
        schedule.Add(now, null);
        schedule.Add("en-US", now, null);
        Assert.Throws<ArgumentException>(() => schedule.Add("en-US", null, now));
        Assert.Throws<ArgumentException>(() => schedule.Add(null, now));
    }

    [Test]
    public void Can_Remove_Invariant()
    {
        var now = DateTime.UtcNow;
        var schedule = new ContentScheduleCollection();
        schedule.Add(now, null);
        var invariantSched = schedule.GetSchedule(Constants.System.InvariantCulture);
        schedule.Remove(invariantSched.First());
        Assert.That(schedule.FullSchedule.Count(), Is.EqualTo(0));
    }

    [Test]
    public void Can_Remove_Variant()
    {
        var now = DateTime.UtcNow;
        var schedule = new ContentScheduleCollection();
        schedule.Add(now, null);
        schedule.Add("en-US", now, null);
        var invariantSched = schedule.GetSchedule(Constants.System.InvariantCulture);
        schedule.Remove(invariantSched.First());
        Assert.That(schedule.GetSchedule(string.Empty).Count(), Is.EqualTo(0));
        Assert.That(schedule.FullSchedule.Count(), Is.EqualTo(1));
        var variantSched = schedule.GetSchedule("en-US");
        schedule.Remove(variantSched.First());
        Assert.That(schedule.GetSchedule("en-US").Count(), Is.EqualTo(0));
        Assert.That(schedule.FullSchedule.Count(), Is.EqualTo(0));
    }

    [Test]
    public void Can_Clear_Start_Invariant()
    {
        var now = DateTime.UtcNow;
        var schedule = new ContentScheduleCollection();
        schedule.Add(now, now.AddDays(1));

        schedule.Clear(ContentScheduleAction.Release);

        Assert.That(schedule.GetSchedule(ContentScheduleAction.Release).Count(), Is.EqualTo(0));
        Assert.That(schedule.GetSchedule(ContentScheduleAction.Expire).Count(), Is.EqualTo(1));
        Assert.That(schedule.FullSchedule.Count(), Is.EqualTo(1));
    }

    [Test]
    public void Can_Clear_End_Variant()
    {
        var now = DateTime.UtcNow;
        var schedule = new ContentScheduleCollection();
        schedule.Add(now, now.AddDays(1));
        schedule.Add("en-US", now, now.AddDays(1));

        schedule.Clear(ContentScheduleAction.Expire);

        Assert.That(schedule.GetSchedule(ContentScheduleAction.Expire).Count(), Is.EqualTo(0));
        Assert.That(schedule.GetSchedule(ContentScheduleAction.Release).Count(), Is.EqualTo(1));
        Assert.That(schedule.GetSchedule("en-US", ContentScheduleAction.Expire).Count(), Is.EqualTo(1));
        Assert.That(schedule.GetSchedule("en-US", ContentScheduleAction.Release).Count(), Is.EqualTo(1));
        Assert.That(schedule.FullSchedule.Count(), Is.EqualTo(3));

        schedule.Clear("en-US", ContentScheduleAction.Expire);

        Assert.That(schedule.GetSchedule(ContentScheduleAction.Expire).Count(), Is.EqualTo(0));
        Assert.That(schedule.GetSchedule(ContentScheduleAction.Release).Count(), Is.EqualTo(1));
        Assert.That(schedule.GetSchedule("en-US", ContentScheduleAction.Expire).Count(), Is.EqualTo(0));
        Assert.That(schedule.GetSchedule("en-US", ContentScheduleAction.Release).Count(), Is.EqualTo(1));
        Assert.That(schedule.FullSchedule.Count(), Is.EqualTo(2));
    }
}
