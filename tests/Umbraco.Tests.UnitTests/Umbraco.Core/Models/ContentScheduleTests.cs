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
    private static readonly DateTime ScheduleDate = new(2026, 10, 26, 12, 0, 0, DateTimeKind.Utc);

    [Test]
    public void Release_Date_Less_Than_Expire_Date()
    {
        var now = DateTime.UtcNow;
        var schedule = new ContentScheduleCollection();
        Assert.IsFalse(schedule.Add(now, now));
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
        Assert.AreEqual(0, schedule.FullSchedule.Count());
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
        Assert.AreEqual(0, schedule.GetSchedule(string.Empty).Count());
        Assert.AreEqual(1, schedule.FullSchedule.Count());
        var variantSched = schedule.GetSchedule("en-US");
        schedule.Remove(variantSched.First());
        Assert.AreEqual(0, schedule.GetSchedule("en-US").Count());
        Assert.AreEqual(0, schedule.FullSchedule.Count());
    }

    [Test]
    public void Can_Clear_Start_Invariant()
    {
        var now = DateTime.UtcNow;
        var schedule = new ContentScheduleCollection();
        schedule.Add(now, now.AddDays(1));

        schedule.Clear(ContentScheduleAction.Release);

        Assert.AreEqual(0, schedule.GetSchedule(ContentScheduleAction.Release).Count());
        Assert.AreEqual(1, schedule.GetSchedule(ContentScheduleAction.Expire).Count());
        Assert.AreEqual(1, schedule.FullSchedule.Count());
    }

    [Test]
    public void Can_Clear_End_Variant()
    {
        var now = DateTime.UtcNow;
        var schedule = new ContentScheduleCollection();
        schedule.Add(now, now.AddDays(1));
        schedule.Add("en-US", now, now.AddDays(1));

        schedule.Clear(ContentScheduleAction.Expire);

        Assert.AreEqual(0, schedule.GetSchedule(ContentScheduleAction.Expire).Count());
        Assert.AreEqual(1, schedule.GetSchedule(ContentScheduleAction.Release).Count());
        Assert.AreEqual(1, schedule.GetSchedule("en-US", ContentScheduleAction.Expire).Count());
        Assert.AreEqual(1, schedule.GetSchedule("en-US", ContentScheduleAction.Release).Count());
        Assert.AreEqual(3, schedule.FullSchedule.Count());

        schedule.Clear("en-US", ContentScheduleAction.Expire);

        Assert.AreEqual(0, schedule.GetSchedule(ContentScheduleAction.Expire).Count());
        Assert.AreEqual(1, schedule.GetSchedule(ContentScheduleAction.Release).Count());
        Assert.AreEqual(0, schedule.GetSchedule("en-US", ContentScheduleAction.Expire).Count());
        Assert.AreEqual(1, schedule.GetSchedule("en-US", ContentScheduleAction.Release).Count());
        Assert.AreEqual(2, schedule.FullSchedule.Count());
    }

    [Test]
    public void Equality_Ignores_Id()
    {
        var schedule = new ContentSchedule(Guid.NewGuid(), "en-US", ScheduleDate, ContentScheduleAction.Release, true);
        var other = new ContentSchedule(Guid.NewGuid(), "en-US", ScheduleDate, ContentScheduleAction.Release, true);

        Assert.IsTrue(schedule.Equals(other));
    }

    [Test]
    public void Equality_Ignores_Culture_Casing()
    {
        var schedule = new ContentSchedule("en-US", ScheduleDate, ContentScheduleAction.Release, true);
        var other = new ContentSchedule("EN-us", ScheduleDate, ContentScheduleAction.Release, true);

        Assert.IsTrue(schedule.Equals(other));
    }

    [TestCase("en-US", "da-DK", false)]
    [TestCase("en-US", "", false)]
    [TestCase("", "", true)]
    public void Equality_Includes_Culture(string first, string second, bool expected)
    {
        var schedule = new ContentSchedule(first, ScheduleDate, ContentScheduleAction.Release, true);
        var other = new ContentSchedule(second, ScheduleDate, ContentScheduleAction.Release, true);

        Assert.AreEqual(expected, schedule.Equals(other));
    }

    [Test]
    public void Equality_Includes_Date()
    {
        var schedule = new ContentSchedule("en-US", ScheduleDate, ContentScheduleAction.Release, true);
        var other = new ContentSchedule("en-US", ScheduleDate.AddTicks(1), ContentScheduleAction.Release, true);

        Assert.IsFalse(schedule.Equals(other));
    }

    [TestCase(ContentScheduleAction.Release, ContentScheduleAction.Release, true)]
    [TestCase(ContentScheduleAction.Expire, ContentScheduleAction.Expire, true)]
    [TestCase(ContentScheduleAction.Release, ContentScheduleAction.Expire, false)]
    public void Equality_Includes_Action(ContentScheduleAction first, ContentScheduleAction second, bool expected)
    {
        var schedule = new ContentSchedule("en-US", ScheduleDate, first, true);
        var other = new ContentSchedule("en-US", ScheduleDate, second, true);

        Assert.AreEqual(expected, schedule.Equals(other));
    }

    [TestCase(true, true, true)]
    [TestCase(false, false, true)]
    [TestCase(true, false, false)]
    [TestCase(false, true, false)]
    public void Equality_Includes_Invariant_For_Variant_Inclusion(bool first, bool second, bool expected)
    {
        var schedule = new ContentSchedule("en-US", ScheduleDate, ContentScheduleAction.Release, first);
        var other = new ContentSchedule("en-US", ScheduleDate, ContentScheduleAction.Release, second);

        Assert.AreEqual(expected, schedule.Equals(other));
    }

    [Test]
    public void Object_Equality_Matches_Typed_Equality()
    {
        var schedule = new ContentSchedule("en-US", ScheduleDate, ContentScheduleAction.Release, true);

        Assert.Multiple(() =>
        {
            Assert.IsTrue(schedule.Equals((object)new ContentSchedule("en-US", ScheduleDate, ContentScheduleAction.Release, true)));
            Assert.IsFalse(schedule.Equals((object)new ContentSchedule("en-US", ScheduleDate, ContentScheduleAction.Release, false)));
            Assert.IsFalse(schedule.Equals((object?)null));
            Assert.IsFalse(schedule.Equals("en-US"));
        });
    }

    [Test]
    public void Deep_Clone_Is_Equal()
    {
        var schedule = new ContentSchedule(Guid.NewGuid(), "en-US", ScheduleDate, ContentScheduleAction.Release, false);

        var clone = (ContentSchedule)schedule.DeepClone();

        Assert.Multiple(() =>
        {
            Assert.AreNotSame(schedule, clone);
            Assert.AreEqual(schedule.Id, clone.Id);
            Assert.IsTrue(schedule.Equals(clone));
        });
    }
}
