// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Core.Models.Validation;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PropertyEditors.Validators;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.PropertyEditors.Validators;

/// <summary>
/// Contains unit tests for the <see cref="IntegerValidator"/> class in the Umbraco CMS property editors validators namespace.
/// </summary>
[TestFixture]
public class IntegerValidatorTests
{
    /// <summary>
    /// Tests the integer validator to ensure it correctly determines whether the provided value is a valid integer.
    /// </summary>
    /// <param name="value">The value to validate; can be null, an empty string, whitespace, or a string representing an integer.</param>
    /// <param name="expectedSuccess">True if the value is expected to be considered a valid integer; otherwise, false.</param>
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
            Assert.IsEmpty(result);
        }
        else
        {
            Assert.AreEqual(1, result.Count());

            var validationResult = result.First();
            Assert.IsTrue(validationResult.ErrorMessage.Contains("is not a valid integer"));
        }
    }

    /// <summary>
    /// Represents the result of validating whether an integer value falls within a specified range.
    /// </summary>
    public enum RangeResult
    {
        Success,
        BelowMin,
        AboveMax,
    }

    /// <summary>
    /// Unit test that verifies whether the <see cref="IntegerValidator"/> correctly validates if the given integer value is within the specified range (10 to 20 inclusive).
    /// </summary>
    /// <param name="value">The value to validate, which may be null or a string representation of an integer.</param>
    /// <param name="expectedResult">The expected result of the range validation, indicating if the value is below the minimum, within range, or above the maximum.</param>
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
            Assert.IsEmpty(result);
        }
        else
        {
            Assert.AreEqual(1, result.Count());

            var validationResult = result.First();
            if (expectedResult == RangeResult.BelowMin)
            {
                Assert.IsTrue(validationResult.ErrorMessage.Contains("less than the minimum allowed value"));
            }
            else if (expectedResult == RangeResult.AboveMax)
            {
                Assert.IsTrue(validationResult.ErrorMessage.Contains("greater than the maximum allowed value"));
            }
        }
    }

    private static IntegerValidator CreateValidator(int? min = null, int? max = null) =>
        min.HasValue is false && max.HasValue is false ? new IntegerValidator() : new IntegerValidator(min, max);
}
