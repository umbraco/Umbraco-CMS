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
public class ColorListValidatorTest
{
    private IConfigurationEditorJsonSerializer ConfigurationEditorJsonSerializer()
        => new SystemTextConfigurationEditorJsonSerializer(new DefaultJsonSerializerEncoderFactory());

    [Test]
    public void Expects_Array_Of_ColorPickerItems_Not_Single_String()
    {
        var validator = new ColorPickerConfigurationEditor.ColorListValidator(ConfigurationEditorJsonSerializer());
        var result =
            validator.Validate(
                "hello",
                null,
                null,
                PropertyValidationContext.Empty());
        Assert.AreEqual(1, result.Count());
    }

    [Test]
    public void Expects_Array_Of_ColorPickerItems_Not_Array_Of_String()
    {
        var validator = new ColorPickerConfigurationEditor.ColorListValidator(ConfigurationEditorJsonSerializer());
        var result =
            validator.Validate(
                new JsonArray("hello", "world"),
                null,
                null,
                PropertyValidationContext.Empty());
        Assert.AreEqual(1, result.Count());
    }

    [Test]
    public void Validates_Color_Vals()
    {
        var validator = new ColorPickerConfigurationEditor.ColorListValidator(ConfigurationEditorJsonSerializer());
        var result =
            validator.Validate(
                new JsonArray(
                    JsonNode.Parse("""{"value": "CC0000", "label": "One"}"""),
                    JsonNode.Parse("""{"value": "zxcvzxcvxzcv", "label": "Two"}"""),
                    JsonNode.Parse("""{"value": "ABC", "label": "Three"}"""),
                    JsonNode.Parse("""{"value": "1234567", "label": "Four"}""")),
                null,
                null,
                PropertyValidationContext.Empty());
        Assert.AreEqual(2, result.Count());
    }

    [Test]
    public void Validates_Color_Vals_Are_Unique()
    {
        var validator = new ColorPickerConfigurationEditor.ColorListValidator(ConfigurationEditorJsonSerializer());
        var result =
            validator.Validate(
                new JsonArray(
                    JsonNode.Parse("""{"value": "FFFFFF", "label": "One"}"""),
                    JsonNode.Parse("""{"value": "000000", "label": "Two"}"""),
                    JsonNode.Parse("""{"value": "FF00AA", "label": "Three"}"""),
                    JsonNode.Parse("""{"value": "fff", "label": "Four"}"""),
                    JsonNode.Parse("""{"value": "000000", "label": "Five"}"""),
                    JsonNode.Parse("""{"value": "F0A", "label": "Six"}""")),
                null,
                null,
                PropertyValidationContext.Empty());
        Assert.AreEqual(1, result.Count());
        Assert.IsTrue(result.First().ErrorMessage.Contains("ffffff, 000000, ff00aa"));
    }

    [Test]
    public void Validates_Color_Can_Contain_Transparency()
    {
        var validator = new ColorPickerConfigurationEditor.ColorListValidator(ConfigurationEditorJsonSerializer());
        var result =
            validator.Validate(
                new JsonArray(
                    JsonNode.Parse("""{"value": "ff000050", "label": "Transparent Red"}"""),
                    JsonNode.Parse("""{"value": "ff0000", "label": "Regular Red"}"""),
                    JsonNode.Parse("""{"value": "ff0000500", "label": "Invalid Red"}""")),
                null,
                null,
                PropertyValidationContext.Empty());
        Assert.AreEqual(1, result.Count());
    }
}
