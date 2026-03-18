using System.Globalization;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models.Validation;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Serialization;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.PropertyEditors;

/// <summary>
/// Contains unit tests for the <see cref="EntityDataPickerPropertyValueEditor"/> class in Umbraco.
/// </summary>
[TestFixture]
public class EntityDataPickerPropertyValueEditorTests
{
    /// <summary>
    /// Validates that the number of selections is greater than or equal to the configured minimum.
    /// </summary>
    /// <param name="numberOfSelections">The number of selections made.</param>
    /// <param name="expectedSuccess">Indicates whether the validation is expected to succeed.</param>
    [TestCase(1, false)]
    [TestCase(2, true)]
    [TestCase(3, true)]
    public void Validates_Is_Greater_Than_Or_Equal_To_Configured_Min(int numberOfSelections, bool expectedSuccess)
    {
        var editor = CreateValueEditor();
        var value = new EntityDataPickerPropertyEditor.EntityDataPickerDto
        {
            Ids = [.. Enumerable.Range(1, numberOfSelections).Select(i => i.ToString())],
        };
        var serializer = new SystemTextJsonSerializer(new DefaultJsonSerializerEncoderFactory());
        var serializedValue = serializer.Serialize(value);
        var result = editor.Validate(serializedValue, false, null, PropertyValidationContext.Empty());
        if (expectedSuccess)
        {
            Assert.IsEmpty(result);
        }
        else
        {
            Assert.AreEqual(1, result.Count());

            var validationResult = result.First();
            Assert.AreEqual("validation_entriesShort", validationResult.ErrorMessage);
        }
    }

    /// <summary>
    /// Validates that the number of selected entities is less than or equal to the configured maximum.
    /// </summary>
    /// <param name="numberOfSelections">The number of entities selected.</param>
    /// <param name="expectedSuccess">Indicates whether the validation is expected to succeed.</param>
    [TestCase(3, true)]
    [TestCase(4, true)]
    [TestCase(5, false)]
    public void Validates_Is_Less_Than_Or_Equal_To_Configured_Max(int numberOfSelections, bool expectedSuccess)
    {
        var editor = CreateValueEditor();
        var value = new EntityDataPickerPropertyEditor.EntityDataPickerDto
        {
            Ids = [.. Enumerable.Range(1, numberOfSelections).Select(i => i.ToString())],
        };
        var serializer = new SystemTextJsonSerializer(new DefaultJsonSerializerEncoderFactory());
        var serializedValue = serializer.Serialize(value);
        var result = editor.Validate(serializedValue, false, null, PropertyValidationContext.Empty());
        if (expectedSuccess)
        {
            Assert.IsEmpty(result);
        }
        else
        {
            Assert.AreEqual(1, result.Count());

            var validationResult = result.First();
            Assert.AreEqual("validation_entriesExceed", validationResult.ErrorMessage);
        }
    }

    private static EntityDataPickerPropertyEditor.EntityDataPickerPropertyValueEditor CreateValueEditor()
    {
        var localizedTextServiceMock = new Mock<ILocalizedTextService>();
        localizedTextServiceMock.Setup(x => x.Localize(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CultureInfo>(),
                It.IsAny<IDictionary<string, string>>()))
            .Returns((string key, string alias, CultureInfo culture, IDictionary<string, string> args) => $"{key}_{alias}");
        return new EntityDataPickerPropertyEditor.EntityDataPickerPropertyValueEditor(
            Mock.Of<IShortStringHelper>(),
            new SystemTextJsonSerializer(new DefaultJsonSerializerEncoderFactory()),
            Mock.Of<IIOHelper>(),
            new DataEditorAttribute("alias"),
            localizedTextServiceMock.Object)
        {
            ConfigurationObject = new EntityDataPickerConfiguration
            {
                DataSource = "testDataSource",
                ValidationLimit = new EntityDataPickerConfiguration.NumberRange
                {
                    Min = 2,
                    Max = 4
                }
            },
        };
    }
}
