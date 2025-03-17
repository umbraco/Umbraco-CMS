using System.ComponentModel.DataAnnotations;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models.Validation;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Serialization;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.PropertyEditors.Validators;

[TestFixture]
internal class TextOnlyValueEditorValidatorTests
{
    internal enum ConfigurationType
    {
        TextAreaConfiguration,
        TextboxConfiguration,
    }

    [TestCase(true, ConfigurationType.TextboxConfiguration, null, "123")]
    [TestCase(true, ConfigurationType.TextAreaConfiguration, null, "123")]
    [TestCase(false, ConfigurationType.TextAreaConfiguration, 2, "123")]
    [TestCase(false, ConfigurationType.TextboxConfiguration, 2, "123")]
    [TestCase(true, ConfigurationType.TextboxConfiguration, 10, "123")]
    [TestCase(true, ConfigurationType.TextAreaConfiguration, 10, "123")]
    public void Validates_String_Length(bool shouldSucceed, ConfigurationType configurationType, int? maxChars, string value)
    {
        var editor = CreateValueEditor();

        editor.ConfigurationObject = CreateConfiguration(configurationType, maxChars);

        var results = editor.Validate(value, false, null, PropertyValidationContext.Empty());

        ValidateResult(shouldSucceed, results);
    }

    private static object CreateConfiguration(ConfigurationType type, int? maxChars) =>
        type switch
        {
            ConfigurationType.TextboxConfiguration => new TextboxConfiguration { MaxChars = maxChars },
            ConfigurationType.TextAreaConfiguration => new TextAreaConfiguration { MaxChars = maxChars },
            _ => throw new InvalidOperationException(),
        };

    private static void ValidateResult(bool succeed, IEnumerable<ValidationResult> result)
    {
        if (succeed)
        {
            Assert.IsEmpty(result);
        }
        else
        {
            Assert.IsNotEmpty(result);
        }
    }

    private TextOnlyValueEditor CreateValueEditor() =>
        new(
            new DataEditorAttribute("alias"),
            Mock.Of<ILocalizedTextService>(),
            Mock.Of<IShortStringHelper>(),
            new SystemTextJsonSerializer(),
            Mock.Of<IIOHelper>());
}
