// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Text.Json.Nodes;
using NUnit.Framework;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Extensions;

[TestFixture]
public class JsonNodeExtensionsTests
{
    [Test]
    public void ToDecimal_Returns_Null_For_Null_Node()
    {
        JsonNode? node = null;

        Assert.That(node.ToDecimal(), Is.Null);
    }

    [TestCase("5", 5d)]
    [TestCase("0", 0d)]
    [TestCase("-3", -3d)]
    [TestCase("5.5", 5.5d)]
    [TestCase("-2.25", -2.25d)]
    [TestCase("1000000", 1000000d)]
    public void ToDecimal_Parses_Numeric_Nodes(string json, double expected)
    {
        JsonNode? node = JsonNode.Parse(json);

        Assert.That(node.ToDecimal(), Is.EqualTo((decimal)expected));
    }

    [TestCase("\"7\"", 7d)]
    [TestCase("\"-2.5\"", -2.5d)]
    public void ToDecimal_Parses_Numeric_String_Nodes(string json, double expected)
    {
        // Legacy configuration sometimes stored numbers as strings; these must still resolve.
        JsonNode? node = JsonNode.Parse(json);

        Assert.That(node.ToDecimal(), Is.EqualTo((decimal)expected));
    }

    [TestCase("\"abc\"")]
    [TestCase("\"\"")]
    [TestCase("true")]
    public void ToDecimal_Returns_Null_For_Non_Numeric_Nodes(string json)
    {
        JsonNode? node = JsonNode.Parse(json);

        Assert.That(node.ToDecimal(), Is.Null);
    }

    [Test]
    public void ToDecimal_Parses_Value_Backed_Node()
    {
        // A node created directly from a CLR value (rather than parsed) exercises the string-parsing fallback.
        JsonNode node = JsonValue.Create(42);

        Assert.That(node.ToDecimal(), Is.EqualTo(42m));
    }
}
