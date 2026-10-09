using System.Text.Json.Nodes;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Patching;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.Patching;

[TestFixture]
public class PatchPathResolverTests
{
    [Test]
    public void Resolve_FilterWithNumericValue_MatchesElement()
    {
        var root = JsonNode.Parse("""
        {
            "items": [
                { "id": 1, "name": "First" },
                { "id": 2, "name": "Second" },
                { "id": 3, "name": "Third" }
            ]
        }
        """)!;

        var segments = new PatchPathSegment[]
        {
            new PropertySegment("items"),
            new FilterSegment([new FilterCondition("id", "2")]),
            new PropertySegment("name"),
        };

        var result = PatchPathResolver.Resolve(root, segments);

        Assert.That(result.Current?.GetValue<string>(), Is.EqualTo("Second"));
    }

    [Test]
    public void Resolve_FilterWithBooleanValue_MatchesElement()
    {
        var root = JsonNode.Parse("""
        {
            "items": [
                { "active": false, "name": "Inactive" },
                { "active": true, "name": "Active" }
            ]
        }
        """)!;

        var segments = new PatchPathSegment[]
        {
            new PropertySegment("items"),
            new FilterSegment([new FilterCondition("active", "true")]),
            new PropertySegment("name"),
        };

        var result = PatchPathResolver.Resolve(root, segments);

        Assert.That(result.Current?.GetValue<string>(), Is.EqualTo("Active"));
    }

    [Test]
    public void Resolve_FilterWithBooleanFalseValue_MatchesElement()
    {
        var root = JsonNode.Parse("""
        {
            "items": [
                { "active": true, "name": "Active" },
                { "active": false, "name": "Inactive" }
            ]
        }
        """)!;

        var segments = new PatchPathSegment[]
        {
            new PropertySegment("items"),
            new FilterSegment([new FilterCondition("active", "false")]),
            new PropertySegment("name"),
        };

        var result = PatchPathResolver.Resolve(root, segments);

        Assert.That(result.Current?.GetValue<string>(), Is.EqualTo("Inactive"));
    }

    [Test]
    public void Resolve_FilterWithDecimalValue_MatchesElement()
    {
        var root = JsonNode.Parse("""
        {
            "items": [
                { "score": 1.5, "name": "Low" },
                { "score": 9.9, "name": "High" }
            ]
        }
        """)!;

        var segments = new PatchPathSegment[]
        {
            new PropertySegment("items"),
            new FilterSegment([new FilterCondition("score", "9.9")]),
            new PropertySegment("name"),
        };

        var result = PatchPathResolver.Resolve(root, segments);

        Assert.That(result.Current?.GetValue<string>(), Is.EqualTo("High"));
    }

    [Test]
    public void Resolve_FilterWithNumericValueNoMatch_Throws()
    {
        var root = JsonNode.Parse("""
        {
            "items": [
                { "id": 1, "name": "First" },
                { "id": 2, "name": "Second" }
            ]
        }
        """)!;

        var segments = new PatchPathSegment[]
        {
            new PropertySegment("items"),
            new FilterSegment([new FilterCondition("id", "99")]),
            new PropertySegment("name"),
        };

        Assert.Throws<InvalidOperationException>(() => PatchPathResolver.Resolve(root, segments));
    }

    [Test]
    public void Resolve_FilterWithMixedStringAndNumericConditions_MatchesElement()
    {
        var root = JsonNode.Parse("""
        {
            "items": [
                { "type": "widget", "priority": 1, "name": "Low Widget" },
                { "type": "widget", "priority": 5, "name": "High Widget" },
                { "type": "gadget", "priority": 5, "name": "High Gadget" }
            ]
        }
        """)!;

        var segments = new PatchPathSegment[]
        {
            new PropertySegment("items"),
            new FilterSegment([new FilterCondition("type", "widget"), new FilterCondition("priority", "5")]),
            new PropertySegment("name"),
        };

        var result = PatchPathResolver.Resolve(root, segments);

        Assert.That(result.Current?.GetValue<string>(), Is.EqualTo("High Widget"));
    }
}
