using System.Globalization;
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
public class RadioButtonsPropertyValueEditorTests
{
    [TestCase("Red", true)]
    [TestCase("Yellow", false)]
    public void Validates_Is_One_Of_Options(object value, bool expectedSuccess)
    {
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
            Assert.AreEqual("validation_notOneOfOptions", validationResult.ErrorMessage);
        }
    }

    [TestCase("Red", true)]
    [TestCase("Rood", false)]
    public void Validates_Against_Option_Values_Not_Labels(object value, bool expectedSuccess)
    {
        var editor = CreateValueEditor();
        editor.ConfigurationObject = new ValueListConfiguration
        {
            Items = [new() { Value = "Red", Label = "Rood" }, new() { Value = "Green", Label = "Groen" }],
        };

        var result = editor.Validate(value, false, null, PropertyValidationContext.Empty());

        Assert.AreEqual(expectedSuccess ? 0 : 1, result.Count());
    }

    private static RadioButtonsPropertyEditor.RadioButtonsPropertyValueEditor CreateValueEditor()
    {
        var localizedTextServiceMock = new Mock<ILocalizedTextService>();
        localizedTextServiceMock.Setup(x => x.Localize(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CultureInfo>(),
                It.IsAny<IDictionary<string, string>>()))
            .Returns((string key, string alias, CultureInfo culture, IDictionary<string, string> args) => $"{key}_{alias}");

        var configuration = new ValueListConfiguration
        {
            Items = [new() { Value = "Red" }, new() { Value = "Green" }, new() { Value = "Blue" }],
        };

        return new RadioButtonsPropertyEditor.RadioButtonsPropertyValueEditor(
            Mock.Of<IShortStringHelper>(),
            Mock.Of<IJsonSerializer>(),
            Mock.Of<IIOHelper>(),
            new DataEditorAttribute("alias"),
            localizedTextServiceMock.Object)
        {
            ConfigurationObject = configuration
        };
    }
}
