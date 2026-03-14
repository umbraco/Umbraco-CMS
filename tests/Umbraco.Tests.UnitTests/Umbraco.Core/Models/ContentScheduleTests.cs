// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Linq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Models;

/// <summary>
/// Contains unit tests for the <see cref="ContentSchedule"/> class in the Umbraco CMS core models.
/// </summary>
[TestFixture]
public class ContentScheduleTests
{
    /// <summary>
    /// Tests that the release date is less than the expire date in the content schedule.
    /// </summary>
    [Test]
    public void Release_Date_Less_Than_Expire_Date()
    {
        var now = DateTime.UtcNow;
        var schedule = new ContentScheduleCollection();
        Assert.IsFalse(schedule.Add(now, now));
    }

    /// <summary>
    /// Tests that adding duplicate dates to the ContentScheduleCollection throws an exception.
    /// </summary>
    [Test]
    public void Cannot_Add_Duplicate_Dates_Invariant()
    {
        var now = DateTime.UtcNow;
        var schedule = new ContentScheduleCollection();
        schedule.Add(now, null);
        Assert.Throws<ArgumentException>(() => schedule.Add(null, now));
    }

    /// <summary>
    /// Tests that adding duplicate dates for the same variant to the content schedule throws an exception.
    /// </summary>
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

    /// <summary>
    /// Tests that an invariant schedule item can be removed from the content schedule collection.
    /// </summary>
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

    /// <summary>
    /// Tests that a variant can be removed from the content schedule.
    /// </summary>
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

    /// <summary>
    /// Tests that the start date can be cleared from the schedule and verifies the schedule counts after clearing.
    /// </summary>
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

    /// <summary>
    /// Verifies that clearing the end (expire) variant of a content schedule removes the correct entries
    /// for both invariant and culture-specific schedules, and that other schedule actions remain unaffected.
    /// The test adds schedules for both invariant and "en-US" variants, clears expire actions, and asserts
    /// the expected counts for each schedule type before and after clearing.
    /// </summary>
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
}
