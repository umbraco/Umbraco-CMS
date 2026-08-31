using NUnit.Framework;
using Umbraco.Cms.Core.PropertyEditors;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.PropertyEditors;

[TestFixture]
public class AllowedContentTypeKeysParserTests
{
    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public void Returns_Empty_When_Nothing_Configured(string? configValue)
        => Assert.That(AllowedContentTypeKeysParser.Parse(configValue), Is.Empty);

    [Test]
    public void Parses_Single_Key()
    {
        var key = Guid.NewGuid();

        HashSet<Guid> result = AllowedContentTypeKeysParser.Parse(key.ToString());

        Assert.That(result, Is.EquivalentTo(new[] { key }));
    }

    [Test]
    public void Parses_Multiple_Comma_Separated_Keys()
    {
        var first = Guid.NewGuid();
        var second = Guid.NewGuid();

        HashSet<Guid> result = AllowedContentTypeKeysParser.Parse($"{first},{second}");

        Assert.That(result, Is.EquivalentTo(new[] { first, second }));
    }

    [Test]
    public void Ignores_Non_Guid_Entries()
    {
        var key = Guid.NewGuid();

        HashSet<Guid> result = AllowedContentTypeKeysParser.Parse($"not-a-guid,{key},also-not-a-guid");

        Assert.That(result, Is.EquivalentTo(new[] { key }));
    }

    [Test]
    public void Ignores_Empty_Entries_From_Extra_Commas()
    {
        var key = Guid.NewGuid();

        HashSet<Guid> result = AllowedContentTypeKeysParser.Parse($",,{key},,");

        Assert.That(result, Is.EquivalentTo(new[] { key }));
    }

    [Test]
    public void Deduplicates_Repeated_Keys()
    {
        var key = Guid.NewGuid();

        HashSet<Guid> result = AllowedContentTypeKeysParser.Parse($"{key},{key}");

        Assert.That(result, Is.EquivalentTo(new[] { key }));
    }
}
