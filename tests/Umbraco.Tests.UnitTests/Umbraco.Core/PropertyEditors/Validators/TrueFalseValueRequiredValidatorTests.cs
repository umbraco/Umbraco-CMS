// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Infrastructure.PropertyEditors.Validators;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.PropertyEditors;

[TestFixture]
public class TrueFalseValueRequiredValidatorTests
{
    [Test]
    public void Validates_Null_Value_As_Not_Provided()
    {
        var validator = new TrueFalseValueRequiredValidator();

        var result = validator.ValidateRequired(null, ValueTypes.Integer);
        Assert.That(result.Count(), Is.EqualTo(1));
    }

    [Test]
    public void Validates_False_Value_As_Not_Provided()
    {
        var validator = new TrueFalseValueRequiredValidator();

        var result = validator.ValidateRequired(false, ValueTypes.Integer);
        Assert.That(result.Count(), Is.EqualTo(1));
    }

    [Test]
    public void Validates_True_Value_As_Provided()
    {
        var validator = new TrueFalseValueRequiredValidator();

        var result = validator.ValidateRequired(true, ValueTypes.Integer);
        Assert.That(result, Is.Empty);
    }
}
