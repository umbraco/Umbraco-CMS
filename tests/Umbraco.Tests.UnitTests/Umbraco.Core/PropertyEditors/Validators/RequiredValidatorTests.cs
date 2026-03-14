// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.ComponentModel.DataAnnotations;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PropertyEditors.Validators;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.PropertyEditors.Validators;

    /// <summary>
    /// Contains unit tests for the <see cref="RequiredValidator"/> class, verifying its validation logic and behavior.
    /// </summary>
[TestFixture]
public class RequiredValidatorTests
{
    /// <summary>
    /// Tests that the RequiredValidator correctly fails validation when the value is null.
    /// </summary>
    [Test]
    public void Validates_Null()
    {
        var validator = new RequiredValidator();
        var result = validator.ValidateRequired(null, ValueTypes.String);
        AssertValidationFailed(result, expectedMessage: Constants.Validation.ErrorMessages.Properties.Missing);
    }

    /// <summary>
    /// Tests the <see cref="RequiredValidator"/> to ensure that string values are correctly validated as required or not.
    /// </summary>
    /// <param name="value">The string value to validate.</param>
    /// <param name="expectedSuccess">True if the value is expected to pass the required validation; otherwise, false.</param>
    [TestCase("", false)]
    [TestCase(" ", false)]
    [TestCase("a", true)]
    public void Validates_Strings(string value, bool expectedSuccess)
    {
        var validator = new RequiredValidator();
        var result = validator.ValidateRequired(value, ValueTypes.String);
        if (expectedSuccess)
        {
            Assert.IsEmpty(result);
        }
        else
        {
            AssertValidationFailed(result);
        }
    }

    /// <summary>
    /// Tests the <see cref="RequiredValidator"/> to ensure it correctly determines whether a JSON string satisfies the 'required' condition.
    /// </summary>
    /// <param name="value">The JSON string to validate.</param>
    /// <param name="expectedSuccess">True if the value is expected to pass the required validation; otherwise, false.</param>
    [TestCase("{}", false)]
    [TestCase("[]", false)]
    [TestCase("{ }", false)]
    [TestCase("[ ]", false)]
    [TestCase(" { } ", false)]
    [TestCase(" [ ] ", false)]
    [TestCase(" { \"foo\": \"bar\" } ", true)]
    public void Validates_Json(string value, bool expectedSuccess)
    {
        var validator = new RequiredValidator();
        var result = validator.ValidateRequired(value, ValueTypes.Json);
        if (expectedSuccess)
        {
            Assert.IsEmpty(result);
        }
        else
        {
            AssertValidationFailed(result);
        }
    }

    private static void AssertValidationFailed(IEnumerable<ValidationResult> result, string expectedMessage = Constants.Validation.ErrorMessages.Properties.Empty)
    {
        Assert.AreEqual(1, result.Count());
        Assert.AreEqual(expectedMessage, result.First().ErrorMessage);
    }
}
