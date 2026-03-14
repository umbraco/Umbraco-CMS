// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Infrastructure.PropertyEditors.Validators;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.PropertyEditors;

/// <summary>
/// Contains unit tests for the <see cref="TrueFalseValueRequiredValidator"/> class, verifying its validation logic and behavior.
/// </summary>
[TestFixture]
public class TrueFalseValueRequiredValidatorTests
{
    /// <summary>
    /// Validates that a null value is treated as not provided by the TrueFalseValueRequiredValidator.
    /// </summary>
    [Test]
    public void Validates_Null_Value_As_Not_Provided()
    {
        var validator = new TrueFalseValueRequiredValidator();

        var result = validator.ValidateRequired(null, ValueTypes.Integer);
        Assert.AreEqual(1, result.Count());
    }

    /// <summary>
    /// Tests that the validator treats a false value as not provided.
    /// </summary>
    [Test]
    public void Validates_False_Value_As_Not_Provided()
    {
        var validator = new TrueFalseValueRequiredValidator();

        var result = validator.ValidateRequired(false, ValueTypes.Integer);
        Assert.AreEqual(1, result.Count());
    }

    /// <summary>
    /// Validates that the value 'true' is considered as provided by the TrueFalseValueRequiredValidator.
    /// </summary>
    [Test]
    public void Validates_True_Value_As_Provided()
    {
        var validator = new TrueFalseValueRequiredValidator();

        var result = validator.ValidateRequired(true, ValueTypes.Integer);
        Assert.IsEmpty(result);
    }
}
