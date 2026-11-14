// Copyright (c) Umbraco.
// See LICENSE for more details.

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
    public void AddOrUpdate_With_The_Same_Date_Keeps_The_User_Who_Scheduled_It()
    {
        var date = DateTime.UtcNow.AddDays(1);
        var id = Guid.NewGuid();
        var schedule = new ContentScheduleCollection();
        schedule.Add(new ContentSchedule(id, "en-US", date, ContentScheduleAction.Release, 1));

        schedule.AddOrUpdate("en-US", date, ContentScheduleAction.Release, 2);

        ContentSchedule entry = schedule.GetSchedule("en-US", ContentScheduleAction.Release).Single();
        Assert.Multiple(() =>
        {
            Assert.AreEqual(1, entry.UserId);
            Assert.AreEqual(id, entry.Id);
        });
    }

    [Test]
    public void AddOrUpdate_With_A_Different_Date_Attributes_The_Entry_To_The_User()
    {
        var date = DateTime.UtcNow.AddDays(1);
        var schedule = new ContentScheduleCollection();
        schedule.Add(new ContentSchedule(Guid.NewGuid(), "en-US", date, ContentScheduleAction.Release, 1));

        schedule.AddOrUpdate("en-US", date.AddDays(1), ContentScheduleAction.Release, 2);

        ContentSchedule entry = schedule.GetSchedule("en-US", ContentScheduleAction.Release).Single();
        Assert.Multiple(() =>
        {
            Assert.AreEqual(2, entry.UserId);
            Assert.AreEqual(date.AddDays(1), entry.Date);
        });
    }

    [Test]
    public void AddOrUpdate_With_The_Same_Date_Attributes_An_Unattributed_Entry_To_The_User()
    {
        var date = DateTime.UtcNow.AddDays(1);
        var schedule = new ContentScheduleCollection();
        schedule.Add(new ContentSchedule(Guid.NewGuid(), "en-US", date, ContentScheduleAction.Release));

        schedule.AddOrUpdate("en-US", date, ContentScheduleAction.Release, 2);

        Assert.AreEqual(2, schedule.GetSchedule("en-US", ContentScheduleAction.Release).Single().UserId);
    }

    [Test]
    public void AttributeUnattributedEntries_Only_Attributes_New_Entries_Without_A_User()
    {
        var date = DateTime.UtcNow.AddDays(1);
        var schedule = new ContentScheduleCollection();
        schedule.Add(new ContentSchedule(Guid.NewGuid(), "en-US", date, ContentScheduleAction.Release));
        schedule.Add(new ContentSchedule("da-DK", date, ContentScheduleAction.Release, 1));
        schedule.Add(new ContentSchedule("de-DE", date, ContentScheduleAction.Release));

        schedule.AttributeUnattributedEntries(2);

        Assert.Multiple(() =>
        {
            Assert.IsNull(schedule.GetSchedule("en-US").Single().UserId);
            Assert.AreEqual(1, schedule.GetSchedule("da-DK").Single().UserId);
            Assert.AreEqual(2, schedule.GetSchedule("de-DE").Single().UserId);
        });
    }

    [Test]
    public void Can_Deep_Clone()
    {
        var schedule = new ContentSchedule(Guid.NewGuid(), "en-US", DateTime.UtcNow, ContentScheduleAction.Expire, 1);

        var clone = (ContentSchedule)schedule.DeepClone();

        Assert.AreNotSame(schedule, clone);
        foreach (var propertyInfo in typeof(ContentSchedule).GetProperties())
        {
            Assert.AreEqual(propertyInfo.GetValue(schedule), propertyInfo.GetValue(clone), propertyInfo.Name);
        }
    }
}
