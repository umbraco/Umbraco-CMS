using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models.Validation;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.PropertyEditors;

[TestFixture]
public class ElementPickerPropertyEditorMinMaxValidationTests
{
    [TestCase(2, null, 3)]
    [TestCase(null, 2, 2)]
    [TestCase(2, 4, 3)]
    public void Can_Pass_Validation_When_Element_Count_Is_Within_Min_Max_Limit(int? min, int? max, int count)
    {
        var result = Validate(min, max, count);

        Assert.IsEmpty(result);
    }

    [Test]
    public void Can_Pass_Validation_When_Limit_Configuration_Is_Null()
    {
        var result = Validate(null, null, 0);

        Assert.IsEmpty(result);
    }

    [Test]
    public void Cannot_Pass_Validation_When_Value_Is_Null_And_Minimum_Is_Required()
    {
        var localizedTextServiceMock = new Mock<ILocalizedTextService>();
        localizedTextServiceMock
            .Setup(x => x.Localize(It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<CultureInfo?>(), It.IsAny<IDictionary<string, string?>?>()))
            .Returns("Validation error.");

        var validator = new ElementPickerPropertyEditor.MinMaxValidator(localizedTextServiceMock.Object);
        var config = new ElementPickerConfiguration
        {
            ValidationLimit = new ElementPickerConfiguration.NumberRange { Min = 1 }
        };

        var result = validator.Validate((List<string>?)null, config, null, PropertyValidationContext.Empty());

        Assert.That(result.Count(), Is.EqualTo(1));
    }

    [TestCase(2, null, 1)]
    [TestCase(null, 2, 3)]
    [TestCase(2, 4, 1)]
    [TestCase(2, 4, 5)]
    public void Cannot_Pass_Validation_When_Element_Count_Is_Outside_Min_Max_Limit(int? min, int? max, int count)
    {
        var result = Validate(min, max, count);

        Assert.That(result.Count(), Is.EqualTo(1));
    }

    private static IEnumerable<ValidationResult> Validate(int? min, int? max, int count)
    {
        var config = new ElementPickerConfiguration
        {
            ValidationLimit = min is null && max is null
                ? null
                : new ElementPickerConfiguration.NumberRange { Min = min, Max = max }
        };

        List<string> value = Enumerable.Range(0, count).Select(_ => Guid.NewGuid().ToString()).ToList();

        var localizedTextServiceMock = new Mock<ILocalizedTextService>();
        localizedTextServiceMock
            .Setup(x => x.Localize(It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<CultureInfo?>(), It.IsAny<IDictionary<string, string?>?>()))
            .Returns("Validation error.");

        var validator = new ElementPickerPropertyEditor.MinMaxValidator(localizedTextServiceMock.Object);
        return validator.Validate(value, config, null, PropertyValidationContext.Empty());
    }
}
