// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Core.Events;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Scoping;

/// <summary>
/// Contains unit tests for the <see cref="EventNameExtractor"/> class.
/// </summary>
[TestFixture]
public class EventNameExtractorTests
{
    /// <summary>
    /// Tests that the EventNameExtractor correctly finds an event with the 'Ing' suffix.
    /// </summary>
    [Test]
    public void Find_Event_Ing()
    {
        var found = EventNameExtractor.FindEvent(this, new SaveEventArgs<string>("test"), EventNameExtractor.MatchIngNames);
        Assert.IsTrue(found.Success);
        Assert.AreEqual("FoundMe", found.Result.Name);
    }

    /// <summary>
    /// Tests that the EventNameExtractor correctly finds event names that do not end with "ing".
    /// </summary>
    [Test]
    public void Find_Event_Non_Ing()
    {
        var found = EventNameExtractor.FindEvent(this, new SaveEventArgs<string>("test"), EventNameExtractor.MatchNonIngNames);
        Assert.IsTrue(found.Success);
        Assert.AreEqual("FindingMe", found.Result.Name);
    }

    /// <summary>
    /// Tests that an ambiguous event match is correctly identified and returns an error.
    /// </summary>
    [Test]
    public void Ambiguous_Match()
    {
        var found = EventNameExtractor.FindEvent(this, new SaveEventArgs<int>(0), EventNameExtractor.MatchIngNames);
        Assert.IsFalse(found.Success);
        Assert.AreEqual(EventNameExtractorError.Ambiguous, found.Result.Error);
    }

    /// <summary>
    /// Tests that no matching event name is found when searching with given arguments.
    /// </summary>
    [Test]
    public void No_Match()
    {
        var found = EventNameExtractor.FindEvent(this, new SaveEventArgs<double>(0), EventNameExtractor.MatchIngNames);
        Assert.IsFalse(found.Success);
        Assert.AreEqual(EventNameExtractorError.NoneFound, found.Result.Error);
    }

    public static event EventHandler<SaveEventArgs<string>> FindingMe;

    public static event EventHandler<SaveEventArgs<string>> FoundMe;

    // will lead to ambiguous matches
    public static event EventHandler<SaveEventArgs<int>> SavingThis;

    public static event EventHandler<SaveEventArgs<int>> SavedThis;

    public static event EventHandler<SaveEventArgs<int>> SavingThat;

    public static event EventHandler<SaveEventArgs<int>> SavedThat;
}
