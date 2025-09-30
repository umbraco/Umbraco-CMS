// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.ComponentModel.DataAnnotations;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PropertyEditors.Validators;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.PropertyEditors.Validators;

[TestFixture]
public class RequiredValidatorTests
{
    [Test]
    public void Validates_Null()
    {
        var validator = new RequiredValidator();
        var result = validator.ValidateRequired(null, ValueTypes.String);
        AssertValidationFailed(result, expectedMessage: Constants.Validation.ErrorMessages.Properties.Missing);
    }

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
