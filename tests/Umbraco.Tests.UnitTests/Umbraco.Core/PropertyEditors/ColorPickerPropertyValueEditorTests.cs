using System.Globalization;
using Humanizer;
using System.Text.Json.Nodes;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models.Validation;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.PropertyEditors;

/// <summary>
/// Contains unit tests for the <see cref="ColorPickerPropertyValueEditor"/> class to verify its behavior and functionality.
/// </summary>
[TestFixture]
public class ColorPickerPropertyValueEditorTests
{
    /// <summary>
    /// Tests whether the specified color string is recognized as a valid, configured color by the ColorPicker property value editor.
    /// </summary>
    /// <param name="color">The color string to validate (e.g., "#ffffff").</param>
    /// <param name="expectedSuccess">True if the color is expected to be valid and configured; otherwise, false.</param>
    /// <remarks>
    /// This test parses the color value into a JSON node, validates it using the value editor, and asserts that the validation result matches the expected outcome.
    /// </remarks>
    [TestCase("#ffffff", true)]
    [TestCase("#f0f0f0", false)]
    public void Validates_Is_Configured_Color(string color, bool expectedSuccess)
    {
        var value = JsonNode.Parse($"{{\"label\": \"\", \"value\": \"{color}\"}}");
        var editor = CreateValueEditor();
        var result = editor.Validate(value, false, null, PropertyValidationContext.Empty());
        if (expectedSuccess)
        {
            Assert.IsEmpty(result);
        }
        else
        {
            Assert.AreEqual(1, result.Count());

            var validationResult = result.First();
            Assert.AreEqual(validationResult.ErrorMessage, "validation_invalidColor");
        }
    }

    private static ColorPickerPropertyEditor.ColorPickerPropertyValueEditor CreateValueEditor()
    {
        var localizedTextServiceMock = new Mock<ILocalizedTextService>();
        localizedTextServiceMock.Setup(x => x.Localize(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CultureInfo>(),
                It.IsAny<IDictionary<string, string>>()))
            .Returns((string key, string alias, CultureInfo culture, IDictionary<string, string> args) => $"{key}_{alias}");
        return new ColorPickerPropertyEditor.ColorPickerPropertyValueEditor(
            Mock.Of<IShortStringHelper>(),
            Mock.Of<IJsonSerializer>(),
            Mock.Of<IIOHelper>(),
            new DataEditorAttribute("alias"),
            localizedTextServiceMock.Object)
        {
            ConfigurationObject = new ColorPickerConfiguration
            {
                Items = [
                    new ColorPickerConfiguration.ColorPickerItem { Value = "ffffff", Label = "White" },
                    new ColorPickerConfiguration.ColorPickerItem { Value = "000000", Label = "Black" }
                ]
            }
        };
    }
}
