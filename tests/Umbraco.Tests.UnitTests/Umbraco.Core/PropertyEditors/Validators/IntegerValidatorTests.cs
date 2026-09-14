// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Core.Models.Validation;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PropertyEditors.Validators;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.PropertyEditors.Validators;

[TestFixture]
public class IntegerValidatorTests
{
    [TestCase(null, true)]
    [TestCase("", true)]
    [TestCase(" ", false)]
    [TestCase("x", false)]
    [TestCase("99", true)]
    public void Validates_Integer(object? value, bool expectedSuccess)
    {
        var validator = CreateValidator();
        var result = validator.Validate(value, ValueTypes.Integer, null, PropertyValidationContext.Empty());
        if (expectedSuccess)
        {
            Assert.That(result, Is.Empty);
        }
        else
        {
            Assert.That(result.Count(), Is.EqualTo(1));

            var validationResult = result.First();
            Assert.That(validationResult.ErrorMessage, Does.Contain("is not a valid integer"));
        }
    }

    public enum RangeResult
    {
        Success,
        BelowMin,
        AboveMax,
    }

    [TestCase("5", RangeResult.BelowMin)]
    [TestCase("10", RangeResult.Success)]
    [TestCase("15", RangeResult.Success)]
    [TestCase("20", RangeResult.Success)]
    [TestCase("25", RangeResult.AboveMax)]
    public void Validates_Integer_Within_Range(object? value, RangeResult expectedResult)
    {
        var validator = CreateValidator(10, 20);
        var result = validator.Validate(value, ValueTypes.Integer, null, PropertyValidationContext.Empty());
        if (expectedResult == RangeResult.Success)
        {
            Assert.That(result, Is.Empty);
        }
        else
        {
            Assert.That(result.Count(), Is.EqualTo(1));

            var validationResult = result.First();
            if (expectedResult == RangeResult.BelowMin)
            {
                Assert.That(validationResult.ErrorMessage, Does.Contain("less than the minimum allowed value"));
            }
            else if (expectedResult == RangeResult.AboveMax)
            {
                Assert.That(validationResult.ErrorMessage, Does.Contain("greater than the maximum allowed value"));
            }
        }
    }

    private static IntegerValidator CreateValidator(int? min = null, int? max = null) =>
        min.HasValue is false && max.HasValue is false ? new IntegerValidator() : new IntegerValidator(min, max);
}
