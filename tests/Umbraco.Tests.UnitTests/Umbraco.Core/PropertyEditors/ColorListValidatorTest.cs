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
        => new SystemTextConfigurationEditorJsonSerializer();

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
}
