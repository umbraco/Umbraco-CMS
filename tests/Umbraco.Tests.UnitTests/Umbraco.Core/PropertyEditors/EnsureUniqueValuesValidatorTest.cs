// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Text.Json.Nodes;
using NUnit.Framework;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Infrastructure.Serialization;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.PropertyEditors;

[TestFixture]
public class EnsureUniqueValuesValidatorTest
{
    private IConfigurationEditorJsonSerializer ConfigurationEditorJsonSerializer()
        => new SystemTextConfigurationEditorJsonSerializer();

    [Test]
    public void Expects_Array_Of_ValueListItems_Not_Single_String()
    {
        var validator = new ValueListUniqueValueValidator(ConfigurationEditorJsonSerializer());
        var result = validator.Validate(
            "hello",
            null,
            null);
        Assert.AreEqual(1, result.Count());
    }

    [Test]
    public void Expects_Array_Of_ValueListItems_Not_Array_Of_String()
    {
        var validator = new ValueListUniqueValueValidator(ConfigurationEditorJsonSerializer());
        var result =
            validator.Validate(
                new JsonArray("hello", "world"),
                null,
                null);
        Assert.AreEqual(1, result.Count());
    }

    [Test]
    public void Allows_Unique_Values()
    {
        var validator = new ValueListUniqueValueValidator(ConfigurationEditorJsonSerializer());
        var result =
            validator.Validate(
                new JsonArray(
                    JsonNode.Parse("""{"value": "hello"}"""),
                    JsonNode.Parse("""{"value": "world"}""")),
                null,
                null);
        Assert.AreEqual(0, result.Count());
    }

    [Test]
    public void Does_Not_Allow_Multiple_Values()
    {
        var validator = new ValueListUniqueValueValidator(ConfigurationEditorJsonSerializer());
        var result =
            validator.Validate(
                new JsonArray(
                    JsonNode.Parse("""{"value": "hello"}"""),
                    JsonNode.Parse("""{"value": "hello"}""")),
                null,
                null);
        Assert.AreEqual(1, result.Count());
    }

    [Test]
    public void Validates_Multiple_Duplicate_Values()
    {
        var validator = new ValueListUniqueValueValidator(ConfigurationEditorJsonSerializer());
        var result =
            validator.Validate(
                new JsonArray(
                    JsonNode.Parse("""{"value": "hello"}"""),
                    JsonNode.Parse("""{"value": "hello"}"""),
                    JsonNode.Parse("""{"value": "world"}"""),
                    JsonNode.Parse("""{"value": "world"}""")),
                null,
                null);
        Assert.AreEqual(2, result.Count());
    }
}
