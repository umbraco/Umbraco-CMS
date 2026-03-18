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

/// <summary>
/// Contains unit tests for the <see cref="RadioButtonsPropertyValueEditor"/> class, verifying its behavior and functionality.
/// </summary>
[TestFixture]
public class RadioButtonsPropertyValueEditorTests
{
    /// <summary>
    /// Tests that the radio button property value editor correctly validates whether the provided value is one of the allowed radio button options.
    /// </summary>
    /// <param name="value">The value to validate against the set of allowed radio button options.</param>
    /// <param name="expectedSuccess">True if the value is expected to be valid (i.e., present in the allowed options); otherwise, false.</param>
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
            Items = ["Red", "Green", "Blue"],
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
