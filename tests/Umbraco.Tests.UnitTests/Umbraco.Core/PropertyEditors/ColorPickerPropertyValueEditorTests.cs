using System.Globalization;
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

[TestFixture]
public class ColorPickerPropertyValueEditorTests
{
    [TestCase("#ffffff", true)]
    [TestCase("#f0f0f0", false)]
    public void Validates_Is_Configured_Color(string color, bool expectedSuccess)
    {
        var value = JsonNode.Parse($"{{\"label\": \"\", \"value\": \"{color}\"}}");
        var editor = CreateValueEditor();
        var result = editor.Validate(value, false, null, PropertyValidationContext.Empty());
        if (expectedSuccess)
        {
            Assert.That(result, Is.Empty);
        }
        else
        {
            Assert.That(result.Count(), Is.EqualTo(1));

            var validationResult = result.First();
            Assert.That(validationResult.ErrorMessage, Is.EqualTo("validation_invalidColor"));
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
