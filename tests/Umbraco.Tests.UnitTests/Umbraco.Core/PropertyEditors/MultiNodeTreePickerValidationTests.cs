using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models.Validation;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Serialization;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.PropertyEditors;

[TestFixture]
public class MultiNodeTreePickerValidationTests
{
    // Remember 0 = no limit
    [TestCase(0, true, "[{\"type\":\"document\",\"unique\":\"86eb02a7-793f-4406-9152-9736b6b64bee\"}]")]
    [TestCase(1, true, "[{\"type\":\"document\",\"unique\":\"86eb02a7-793f-4406-9152-9736b6b64bee\"}]")]
    [TestCase(2, true, "[{\"type\":\"document\",\"unique\":\"86eb02a7-793f-4406-9152-9736b6b64bee\"},{\"type\":\"document\",\"unique\":\"25ef6fd2-db48-450a-8c48-df3ad75adf4b\"}]")]
    [TestCase(3, false, "[{\"type\":\"document\",\"unique\":\"86eb02a7-793f-4406-9152-9736b6b64bee\"},{\"type\":\"document\",\"unique\":\"86eb02a7-793f-4406-9152-9736b6b64bee\"}]")]
    [TestCase(2, false, "[{\"type\":\"document\",\"unique\":\"86eb02a7-793f-4406-9152-9736b6b64bee\"}]")]
    public void Validates_Minimum_Entries(int min, bool shouldSucceed, string value)
    {
        var valueEditor = CreateValueEditor();
        valueEditor.ConfigurationObject = new MultiNodePickerConfiguration { MinNumber = min};

        var result = valueEditor.Validate(value, false, null, PropertyValidationContext.Empty());

        if (shouldSucceed)
        {
            Assert.IsEmpty(result);
        }
        else
        {
            Assert.IsNotEmpty(result);
        }
    }

    [TestCase(0, true, "[]")]
    [TestCase(1, true, "[{\"type\":\"document\",\"unique\":\"86eb02a7-793f-4406-9152-9736b6b64bee\"}]")]
    [TestCase(0, true, "[{\"type\":\"document\",\"unique\":\"86eb02a7-793f-4406-9152-9736b6b64bee\"}]")]
    [TestCase(1, false, "[{\"type\":\"document\",\"unique\":\"86eb02a7-793f-4406-9152-9736b6b64bee\"},{\"type\":\"document\",\"unique\":\"25ef6fd2-db48-450a-8c48-df3ad75adf4b\"}]")]
    [TestCase(3, true, "[{\"type\":\"document\",\"unique\":\"86eb02a7-793f-4406-9152-9736b6b64bee\"},{\"type\":\"document\",\"unique\":\"86eb02a7-793f-4406-9152-9736b6b64bee\"}]")]
    [TestCase(2, true, "[{\"type\":\"document\",\"unique\":\"86eb02a7-793f-4406-9152-9736b6b64bee\"}]")]
    public void Validates_Maximum_Entries(int max, bool shouldSucceed, string value)
    {
        var valueEditor = CreateValueEditor();
        valueEditor.ConfigurationObject = new MultiNodePickerConfiguration { MaxNumber = max };

        var result = valueEditor.Validate(value, false, null, PropertyValidationContext.Empty());

        if (shouldSucceed)
        {
            Assert.IsEmpty(result);
        }
        else
        {
            Assert.IsNotEmpty(result);
        }
    }

    private static MultiNodeTreePickerPropertyEditor.MultiNodeTreePickerPropertyValueEditor CreateValueEditor()
    {
        var valueEditor = new MultiNodeTreePickerPropertyEditor.MultiNodeTreePickerPropertyValueEditor(
            Mock.Of<IShortStringHelper>(),
            new SystemTextJsonSerializer(),
            Mock.Of<IIOHelper>(),
            new DataEditorAttribute("alias"),
            Mock.Of<ILocalizedTextService>()
        )
        {
            ConfigurationObject = new MultiNodePickerConfiguration(),
        };

        return valueEditor;
    }
}
