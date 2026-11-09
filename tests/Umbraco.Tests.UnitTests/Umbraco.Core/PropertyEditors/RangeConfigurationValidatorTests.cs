using System.ComponentModel.DataAnnotations;
using System.Text.Json.Nodes;
using NUnit.Framework;
using Umbraco.Cms.Core.Models.Validation;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PropertyEditors.Validators;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.PropertyEditors;

[TestFixture]
public class RangeConfigurationValidatorTests
{
    private readonly RangeConfigurationValidator _validator = new();

    private ValidationResult[] Validate(object? value)
        => _validator.Validate(value, null, null, PropertyValidationContext.Empty()).ToArray();

    [Test]
    public void Cannot_Validate_When_Max_Is_Less_Than_Min()
    {
        var value = new JsonObject { ["min"] = 5, ["max"] = 2 };

        Assert.That(Validate(value), Has.Length.EqualTo(1));
    }

    [Test]
    public void Can_Validate_When_Max_Is_Greater_Than_Min()
    {
        var value = new JsonObject { ["min"] = 2, ["max"] = 5 };

        Assert.That(Validate(value), Is.Empty);
    }

    [Test]
    public void Can_Validate_When_Min_Equals_Max()
    {
        var value = new JsonObject { ["min"] = 3, ["max"] = 3 };

        Assert.That(Validate(value), Is.Empty);
    }

    [TestCase(null, 5)]
    [TestCase(5, null)]
    [TestCase(null, null)]
    public void Can_Validate_When_A_Bound_Is_Unset(int? min, int? max)
    {
        var value = new JsonObject();
        if (min.HasValue)
        {
            value["min"] = min.Value;
        }

        if (max.HasValue)
        {
            value["max"] = max.Value;
        }

        Assert.That(Validate(value), Is.Empty);
    }

    [Test]
    public void Cannot_Validate_Typed_NumberRange_When_Max_Is_Less_Than_Min()
    {
        var value = new NumberRange { Min = 10, Max = 1 };

        Assert.That(Validate(value), Has.Length.EqualTo(1));
    }

    [Test]
    public void Cannot_Validate_Typed_DecimalRange_When_Max_Is_Less_Than_Min()
    {
        var value = new DecimalRange { Min = 1.5m, Max = 0.5m };

        Assert.That(Validate(value), Has.Length.EqualTo(1));
    }

    [Test]
    public void Can_Validate_When_Value_Is_Not_A_Range()
    {
        Assert.That(Validate(null), Is.Empty);
        Assert.That(Validate("not-a-range"), Is.Empty);
    }
}
