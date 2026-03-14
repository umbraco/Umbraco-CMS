using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Configuration.Models;

/// <summary>
/// Contains unit tests for the <see cref="RequestHandlerSettings"/> configuration model in Umbraco.
/// </summary>
[TestFixture]
public class RequestHandlerSettingsTests
{
    /// <summary>
    /// Tests that the character collection merges correctly when default is enabled.
    /// </summary>
    [Test]
    public void Given_CharCollection_With_DefaultEnabled_MergesCollection()
    {
        var settings = new RequestHandlerSettings
        {
            UserDefinedCharCollection =
            [
                new() { Char = "test", Replacement = "replace" },
                new() { Char = "test2", Replacement = "replace2" },
            ]
        };
        var actual = settings.GetCharReplacements().ToList();

        var expectedCollection = RequestHandlerSettings.DefaultCharCollection.ToList();
        expectedCollection.AddRange(settings.UserDefinedCharCollection);

        Assert.AreEqual(expectedCollection.Count, actual.Count);
        Assert.That(actual, Is.EquivalentTo(expectedCollection));
    }

    /// <summary>
    /// Tests that when the default character replacements are disabled,
    /// the user-defined character collection is returned correctly.
    /// </summary>
    [Test]
    public void Given_CharCollection_With_DefaultDisabled_ReturnsUserCollection()
    {
        var settings = new RequestHandlerSettings
        {
            UserDefinedCharCollection =
            [
                new() { Char = "test", Replacement = "replace" },
                new() { Char = "test2", Replacement = "replace2" },
            ],
            EnableDefaultCharReplacements = false,
        };
        var actual = settings.GetCharReplacements().ToList();

        Assert.AreEqual(settings.UserDefinedCharCollection.Count(), actual.Count);
        Assert.That(actual, Is.EquivalentTo(settings.UserDefinedCharCollection));
    }

    /// <summary>
    /// Tests that when the UserDefinedCharCollection overrides the default values,
    /// the GetCharReplacements method returns the correct replacements including the overrides.
    /// </summary>
    [Test]
    public void Given_CharCollection_That_OverridesDefaultValues_ReturnsReplacements()
    {
        var settings = new RequestHandlerSettings
        {
            UserDefinedCharCollection =
            [
                new() { Char = "%", Replacement = "percent" },
                new() { Char = ".", Replacement = "dot" },
            ]
        };
        var actual = settings.GetCharReplacements().ToList();

        Assert.AreEqual(RequestHandlerSettings.DefaultCharCollection.Length, actual.Count);

        Assert.That(actual, Has.Exactly(1).Matches<CharItem>(x => x.Char == "%" && x.Replacement == "percent"));
        Assert.That(actual, Has.Exactly(1).Matches<CharItem>(x => x.Char == "." && x.Replacement == "dot"));
        Assert.That(actual, Has.Exactly(0).Matches<CharItem>(x => x.Char == "%" && x.Replacement == string.Empty));
        Assert.That(actual, Has.Exactly(0).Matches<CharItem>(x => x.Char == "." && x.Replacement == string.Empty));
    }

    /// <summary>
    /// Tests that when a character collection overrides the default values and contains new entries,
    /// the merged result includes the replacements correctly.
    /// </summary>
    [Test]
    public void Given_CharCollection_That_OverridesDefaultValues_And_ContainsNew_ReturnsMergedWithReplacements()
    {
        var settings = new RequestHandlerSettings
        {
            UserDefinedCharCollection =
            [
                new() { Char = "%", Replacement = "percent" },
                new() { Char = ".", Replacement = "dot" },
                new() { Char = "new", Replacement = "new" },
            ]
        };
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
