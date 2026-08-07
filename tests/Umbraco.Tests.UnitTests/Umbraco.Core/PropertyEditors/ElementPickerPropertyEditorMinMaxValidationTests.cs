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
    private ElementPickerPropertyEditor.MinMaxValidator _validator = null!;

    [SetUp]
    public void SetUp()
    {
        var localizedTextServiceMock = new Mock<ILocalizedTextService>();
        localizedTextServiceMock
            .Setup(x => x.Localize(It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<CultureInfo?>(), It.IsAny<IDictionary<string, string?>?>()))
            .Returns("Validation error.");

        _validator = new ElementPickerPropertyEditor.MinMaxValidator(localizedTextServiceMock.Object);
    }

    [TestCase(2, null, 3)]
    [TestCase(null, 2, 2)]
    [TestCase(2, 4, 3)]
    public void Can_Pass_Validation_When_Element_Count_Is_Within_Min_Max_Limit(int? min, int? max, int count)
        => Assert.IsEmpty(Validate(min, max, count));

    [Test]
    public void Can_Pass_Validation_When_Limit_Configuration_Is_Null()
        => Assert.IsEmpty(Validate(null, null, 0));

    [Test]
    public void Cannot_Pass_Validation_When_Value_Is_Null_And_Minimum_Is_Required()
    {
        var config = new ElementPickerConfiguration
        {
            ValidationLimit = new NumberRange { Min = 1 },
        };

        Assert.That(_validator.Validate((List<string>?)null, config, null, PropertyValidationContext.Empty()).Count(), Is.EqualTo(1));
    }

    [TestCase(2, null, 1)]
    [TestCase(null, 2, 3)]
    [TestCase(2, 4, 1)]
    [TestCase(2, 4, 5)]
    public void Cannot_Pass_Validation_When_Element_Count_Is_Outside_Min_Max_Limit(int? min, int? max, int count)
        => Assert.That(Validate(min, max, count).Count(), Is.EqualTo(1));

    private IEnumerable<ValidationResult> Validate(int? min, int? max, int count)
    {
        var config = new ElementPickerConfiguration
        {
            ValidationLimit = min is null && max is null
                ? null
                : new NumberRange { Min = min, Max = max },
        };

        List<string> value = Enumerable.Range(0, count).Select(_ => Guid.NewGuid().ToString()).ToList();

        return _validator.Validate(value, config, null, PropertyValidationContext.Empty());
    }
}
