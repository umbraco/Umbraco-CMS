// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Core.Events;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Scoping;

[TestFixture]
public class EventNameExtractorTests
{
    [Test]
    public void Find_Event_Ing()
    {
        var found = EventNameExtractor.FindEvent(this, new SaveEventArgs<string>("test"), EventNameExtractor.MatchIngNames);
        Assert.That(found.Success, Is.True);
        Assert.That(found.Result.Name, Is.EqualTo("FoundMe"));
    }

    [Test]
    public void Find_Event_Non_Ing()
    {
        var found = EventNameExtractor.FindEvent(this, new SaveEventArgs<string>("test"), EventNameExtractor.MatchNonIngNames);
        Assert.That(found.Success, Is.True);
        Assert.That(found.Result.Name, Is.EqualTo("FindingMe"));
    }

    [Test]
    public void Ambiguous_Match()
    {
        var found = EventNameExtractor.FindEvent(this, new SaveEventArgs<int>(0), EventNameExtractor.MatchIngNames);
        Assert.That(found.Success, Is.False);
        Assert.That(found.Result.Error, Is.EqualTo(EventNameExtractorError.Ambiguous));
    }

    [Test]
    public void No_Match()
    {
        var found = EventNameExtractor.FindEvent(this, new SaveEventArgs<double>(0), EventNameExtractor.MatchIngNames);
        Assert.That(found.Success, Is.False);
        Assert.That(found.Result.Error, Is.EqualTo(EventNameExtractorError.NoneFound));
    }

    public static event EventHandler<SaveEventArgs<string>> FindingMe { add { } remove { } }

    public static event EventHandler<SaveEventArgs<string>> FoundMe { add { } remove { } }

    // will lead to ambiguous matches
    public static event EventHandler<SaveEventArgs<int>> SavingThis { add { } remove { } }

    public static event EventHandler<SaveEventArgs<int>> SavedThis { add { } remove { } }

    public static event EventHandler<SaveEventArgs<int>> SavingThat { add { } remove { } }

    public static event EventHandler<SaveEventArgs<int>> SavedThat { add { } remove { } }
}
