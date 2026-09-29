using System.ComponentModel.DataAnnotations;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.Validation;
using Umbraco.Cms.Core.PropertyEditors.Validation;
using Umbraco.Cms.Infrastructure.Serialization;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.PropertyEditors.Validation;

[TestFixture]
public class TypedJsonValidatorRunnerTests
{
    [Test]
    public void Runs_Validators_For_Value_Of_Expected_Shape()
    {
        var validator = new TestValidator();

        var result = Run(validator, "{\"myProperty\":\"value\"}");

        Assert.IsTrue(validator.WasRun);
        Assert.IsEmpty(result);
    }

    [Test]
    public void Runs_Validators_For_Null_Value()
    {
        var validator = new TestValidator();

        var result = Run(validator, null);

        Assert.IsTrue(validator.WasRun);
        Assert.IsEmpty(result);
    }

    [TestCase("[]")]
    [TestCase("{\"anotherProperty\":\"value\"}")]
    public void Reports_Json_Value_Of_Unexpected_Shape_As_Invalid(string value)
    {
        var validator = new TestValidator();

        var result = Run(validator, value).ToArray();

        Assert.IsFalse(validator.WasRun);
        Assert.AreEqual(1, result.Length);
        Assert.AreEqual(Constants.Validation.ErrorMessages.Properties.Invalid, result.First().ErrorMessage);
    }

    [Test]
    public void Skips_Value_That_Is_Not_Json()
    {
        // A persisted value is not necessarily in the editor form these validators apply to, so it must not be
        // reported as invalid.
        var validator = new TestValidator();

        var result = Run(validator, "umb://document/8f1ef1e8bb2d4f2d9a2c9e2b0f1a3c4d");

        Assert.IsFalse(validator.WasRun);
        Assert.IsEmpty(result);
    }

    private static IEnumerable<ValidationResult> Run(TestValidator validator, object? value)
    {
        var runner = new TypedJsonValidatorRunner<TestValue, TestConfiguration>(
            new SystemTextJsonSerializer(new DefaultJsonSerializerEncoderFactory()),
            validator);

        return runner.Validate(value, null, new TestConfiguration(), PropertyValidationContext.Empty());
    }

    private class TestValue
    {
        public required string MyProperty { get; set; }
    }

    private class TestConfiguration
    {
    }

    private class TestValidator : ITypedJsonValidator<TestValue, TestConfiguration>
    {
        public bool WasRun { get; private set; }

        public IEnumerable<ValidationResult> Validate(
            TestValue? value,
            TestConfiguration? configuration,
            string? valueType,
            PropertyValidationContext validationContext)
        {
            WasRun = true;
            return [];
        }
    }
}
