// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Text.Json.Nodes;
using NUnit.Framework;
using Umbraco.Cms.Core.Models.Validation;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Infrastructure.Serialization;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.PropertyEditors;

[TestFixture]
public class EnsureUniqueValuesValidatorTest
{
    private IConfigurationEditorJsonSerializer ConfigurationEditorJsonSerializer()
        => new SystemTextConfigurationEditorJsonSerializer(new DefaultJsonSerializerEncoderFactory());

    // Reads the items the way configuration data is read from the database, so they have the same runtime type.
    private object DeserializeItems(string itemsJson)
        => ConfigurationEditorJsonSerializer().Deserialize<Dictionary<string, object>>($$"""{"items":{{itemsJson}}}""")!["items"];

    private static JsonArray Options(params string[] values)
        => new(values.Select(value => (JsonNode)new JsonObject { ["value"] = value }).ToArray());

    private IEnumerable<System.ComponentModel.DataAnnotations.ValidationResult> Validate(object? value)
        => new ValueListUniqueValueValidator(ConfigurationEditorJsonSerializer())
            .Validate(value, null, null, PropertyValidationContext.Empty());

    [Test]
    public void Expects_Array_Of_Options_Not_Single_String()
        => Assert.AreEqual(1, Validate("hello").Count());

    [Test]
    public void Rejects_Plain_String_Items()
        => Assert.AreEqual(1, Validate(new JsonArray("hello", "world")).Count());

    [Test]
    public void Allows_Unique_Values()
        => Assert.AreEqual(0, Validate(Options("one", "two", "three")).Count());

    [Test]
    public void Rejects_Duplicate_Value()
        => Assert.AreEqual(1, Validate(Options("one", "one")).Count());

    [Test]
    public void Validates_Multiple_Duplicate_Values()
        => Assert.AreEqual(2, Validate(Options("one", "two", "three", "one", "two")).Count());

    [Test]
    public void Handles_Null()
        => Assert.AreEqual(0, Validate(null).Count());

    [Test]
    public void Handles_Collections_Of_Options()
    {
        var options = new[] { new ValueListOption { Value = "one" }, new ValueListOption { Value = "two" } };

        Assert.Multiple(() =>
        {
            Assert.AreEqual(0, Validate(options).Count());
            Assert.AreEqual(0, Validate(options.ToList()).Count());
            Assert.AreEqual(0, Validate((IEnumerable<ValueListOption>)options).Count());
        });
    }

    [Test]
    public void Accepts_Options_With_Labels()
    {
        var items = new JsonArray(
            new JsonObject { ["value"] = "one", ["label"] = "One" },
            new JsonObject { ["value"] = "two" },
            new JsonObject { ["value"] = "three", ["label"] = "Three" });

        Assert.AreEqual(0, Validate(items).Count());
    }

    [Test]
    public void Accepts_Options_As_Read_From_The_Database()
        => Assert.AreEqual(
            0,
            Validate(DeserializeItems("""[{"value":"one"},{"value":"two","label":"Two"},{"value":"three"}]""")).Count());

    [Test]
    public void Rejects_Duplicate_Value_With_Different_Labels()
        => Assert.AreEqual(
            2,
            Validate(DeserializeItems("""[{"value":"one"},{"value":"one","label":"One"},{"value":"two"},{"value":"two"},{"value":"three"}]""")).Count());
}
