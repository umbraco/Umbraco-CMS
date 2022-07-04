using System.Linq;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Configuration.Models;

[TestFixture]
public class RequestHandlerSettingsTests
{
    [Test]
    public void Given_CharCollection_With_DefaultEnabled_MergesCollection()
    {
        var userCollection = new CharItem[]
        {
            new() { Char = "test", Replacement = "replace" },
            new() { Char = "test2", Replacement = "replace2" },
        };

        var settings = new RequestHandlerSettings { UserDefinedCharCollection = userCollection };
        var actual = settings.GetCharReplacements().ToList();

        var expectedCollection = RequestHandlerSettings.DefaultCharCollection.ToList();
        expectedCollection.AddRange(userCollection);

        Assert.AreEqual(expectedCollection.Count, actual.Count);
        Assert.That(actual, Is.EquivalentTo(expectedCollection));
    }

    [Test]
    public void Given_CharCollection_With_DefaultDisabled_ReturnsUserCollection()
    {
        var userCollection = new CharItem[]
        {
            new() { Char = "test", Replacement = "replace" },
            new() { Char = "test2", Replacement = "replace2" },
        };

        var settings = new RequestHandlerSettings
        {
            UserDefinedCharCollection = userCollection,
            EnableDefaultCharReplacements = false,
        };
        var actual = settings.GetCharReplacements().ToList();

        Assert.AreEqual(userCollection.Length, actual.Count);
        Assert.That(actual, Is.EquivalentTo(userCollection));
    }

    [Test]
    public void Given_CharCollection_That_OverridesDefaultValues_ReturnsReplacements()
    {
        var userCollection = new CharItem[]
        {
            new() { Char = "%", Replacement = "percent" },
            new() { Char = ".", Replacement = "dot" },
        };

        var settings = new RequestHandlerSettings { UserDefinedCharCollection = userCollection };
        var actual = settings.GetCharReplacements().ToList();

        Assert.AreEqual(RequestHandlerSettings.DefaultCharCollection.Length, actual.Count);

        Assert.That(actual, Has.Exactly(1).Matches<CharItem>(x => x.Char == "%" && x.Replacement == "percent"));
        Assert.That(actual, Has.Exactly(1).Matches<CharItem>(x => x.Char == "." && x.Replacement == "dot"));
        Assert.That(actual, Has.Exactly(0).Matches<CharItem>(x => x.Char == "%" && x.Replacement == string.Empty));
        Assert.That(actual, Has.Exactly(0).Matches<CharItem>(x => x.Char == "." && x.Replacement == string.Empty));
    }

    [Test]
    public void Given_CharCollection_That_OverridesDefaultValues_And_ContainsNew_ReturnsMergedWithReplacements()
    {
        var userCollection = new CharItem[]
        {
            new() { Char = "%", Replacement = "percent" },
            new() { Char = ".", Replacement = "dot" },
            new() { Char = "new", Replacement = "new" },
        };

        var settings = new RequestHandlerSettings { UserDefinedCharCollection = userCollection };
        var actual = settings.GetCharReplacements().ToList();

        // Add 1 to the length, because we're expecting to only add one new one
        Assert.AreEqual(RequestHandlerSettings.DefaultCharCollection.Length + 1, actual.Count);

        Assert.That(actual, Has.Exactly(1).Matches<CharItem>(x => x.Char == "%" && x.Replacement == "percent"));
        Assert.That(actual, Has.Exactly(1).Matches<CharItem>(x => x.Char == "." && x.Replacement == "dot"));
        Assert.That(actual, Has.Exactly(1).Matches<CharItem>(x => x.Char == "new" && x.Replacement == "new"));
        Assert.That(actual, Has.Exactly(0).Matches<CharItem>(x => x.Char == "%" && x.Replacement == string.Empty));
        Assert.That(actual, Has.Exactly(0).Matches<CharItem>(x => x.Char == "." && x.Replacement == string.Empty));
    }
}
