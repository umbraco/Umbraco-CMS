using System.Globalization;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Editors;
using Umbraco.Cms.Core.Models.Validation;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.PropertyEditors;

/// <summary>
/// Contains unit tests for the <see cref="IntegerPropertyValueEditor"/> class in Umbraco CMS.
/// </summary>
[TestFixture]
public class IntegerPropertyValueEditorTests
{
    // annoyingly we can't use decimals etc. in attributes, so we can't turn these into test cases :(
    private Dictionary<object?,object?> _valuesAndExpectedResults = new();

    /// <summary>
    /// Initializes the test data mapping various input values to their expected integer conversion results
    /// for the integer property value editor tests.
    /// </summary>
    [SetUp]
    public void SetUp() => _valuesAndExpectedResults = new Dictionary<object?, object?>
    {
        { 123m, 123 },
        { 123, 123 },
        { -123, -123 },
        { 123.45d, null },
        { "123.45", null },
        { "1234.56", null },
        { "123,45", null },
        { "1.234,56", null },
        { "123 45", null },
        { "something", null },
        { true, null },
        { new object(), null },
        { new List<string> { "some", "values" }, null },
        { Guid.NewGuid(), null },
        { new GuidUdi(Constants.UdiEntityType.Document, Guid.NewGuid()), null }
    };

    /// <summary>
    /// Tests that values can be correctly parsed from the editor input.
    /// </summary>
    [Test]
    public void Can_Parse_Values_From_Editor()
    {
        foreach (var (value, expected) in _valuesAndExpectedResults)
        {
            var fromEditor = FromEditor(value);
            Assert.AreEqual(expected, fromEditor, message: $"Failed for: {value}");
        }
    }

    /// <summary>
    /// Tests that various values can be correctly parsed to the editor format.
    /// </summary>
    [Test]
    public void Can_Parse_Values_To_Editor()
    {
        foreach (var (value, expected) in _valuesAndExpectedResults)
        {
            var toEditor = ToEditor(value);
            Assert.AreEqual(expected, toEditor, message: $"Failed for: {value}");
        }
    }

    /// <summary>
    /// Tests that passing null from the editor returns null.
    /// </summary>
    [Test]
    public void Null_From_Editor_Yields_Null()
    {
        var result = FromEditor(null);
        Assert.IsNull(result);
    }

    /// <summary>
    /// Tests that converting a null value to the editor yields a null result.
    /// </summary>
    [Test]
    public void Null_To_Editor_Yields_Null()
    {
        var result = ToEditor(null);
        Assert.IsNull(result);
    }

    /// <summary>
    /// Tests that the integer property value editor correctly validates whether a value is an integer.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <param name="expectedSuccess">Indicates whether the validation is expected to succeed.</param>
    [TestCase("x", false)]
    [TestCase(10, true)]
    public void Validates_Is_Integer(object value, bool expectedSuccess)
    {
        var editor = CreateValueEditor();
        var result = editor.Validate(value, false, null, PropertyValidationContext.Empty());
        if (expectedSuccess)
        {
            Assert.IsEmpty(result);
        }
        else
        {
            Assert.AreEqual(1, result.Count());

            var validationResult = result.First();
            Assert.AreEqual($"The value {value} is not a valid integer", validationResult.ErrorMessage);
        }
    }

    /// <summary>
    /// Tests that the integer property value editor correctly validates whether the provided value
    /// is greater than or equal to the configured minimum value.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <param name="expectedSuccess">True if the value is expected to pass validation; otherwise, false.</param>
    [TestCase(8, false)]
    [TestCase(10, true)]
    [TestCase(12, true)]
    public void Validates_Is_Greater_Than_Or_Equal_To_Configured_Min(object value, bool expectedSuccess)
    {
        var editor = CreateValueEditor();
        var result = editor.Validate(value, false, null, PropertyValidationContext.Empty());
        if (expectedSuccess)
        {
            Assert.IsEmpty(result);
        }
        else
        {
            Assert.AreEqual(1, result.Count());

            var validationResult = result.First();
            Assert.AreEqual("validation_outOfRangeMinimum", validationResult.ErrorMessage);
        }
    }

    /// <summary>
    /// Verifies that the integer property value editor correctly validates input values against a configured maximum value.
    /// The test checks that values less than or equal to the maximum pass validation, while values greater than the maximum fail validation.
    /// </summary>
    /// <param name="value">The value to be validated by the property value editor.</param>
    /// <param name="expectedSuccess">True if the value is expected to pass validation; false if it is expected to fail.</param>
    [TestCase(18, true)]
    [TestCase(20, true)]
    [TestCase(22, false)]
    public void Validates_Is_Less_Than_Or_Equal_To_Configured_Max(object value, bool expectedSuccess)
    {
        var editor = CreateValueEditor();
        var result = editor.Validate(value, false, null, PropertyValidationContext.Empty());
        if (expectedSuccess)
        {
            Assert.IsEmpty(result);
        }
        else
        {
            Assert.AreEqual(1, result.Count());

            var validationResult = result.First();
            Assert.AreEqual("validation_outOfRangeMaximum", validationResult.ErrorMessage);
        }
    }

    /// <summary>
    /// Tests that the value is validated against the configured step increment for integer property values.
    /// If the step is zero, validation always succeeds, as any value is considered valid.
    /// </summary>
    /// <param name="step">The step increment to validate against. If zero, validation always passes.</param>
    /// <param name="value">The value to validate.</param>
    /// <param name="expectedSuccess">True if validation is expected to succeed; otherwise, false.</param>
    [TestCase(2, 17, false)]
    [TestCase(2, 18, true)]
    [TestCase(0, 17, true)] // A step of zero would trigger a divide by zero error in evaluating. So we always pass validation for zero, as effectively any step value is valid.
    public void Validates_Matches_Configured_Step(int step, object value, bool expectedSuccess)
    {
        var editor = CreateValueEditor(step: step);
        var result = editor.Validate(value, false, null, PropertyValidationContext.Empty());
        if (expectedSuccess)
        {
            Assert.IsEmpty(result);
        }
        else
        {
            Assert.AreEqual(1, result.Count());

            var validationResult = result.First();
            Assert.AreEqual("validation_invalidStep", validationResult.ErrorMessage);
        }
    }

    private static object? FromEditor(object? value)
        => CreateValueEditor().FromEditor(new ContentPropertyData(value, null), null);

    private static object? ToEditor(object? value)
    {
        var property = new Mock<IProperty>();
        property
            .Setup(p => p.GetValue(It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<bool>()))
            .Returns(value);

        return CreateValueEditor().ToEditor(property.Object);
    }

    private static IntegerPropertyEditor.IntegerPropertyValueEditor CreateValueEditor(int step = 2)
    {
        var localizedTextServiceMock = new Mock<ILocalizedTextService>();
        localizedTextServiceMock.Setup(x => x.Localize(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CultureInfo>(),
                It.IsAny<IDictionary<string, string>>()))
            .Returns((string key, string alias, CultureInfo culture, IDictionary<string, string> args) => $"{key}_{alias}");
        return new IntegerPropertyEditor.IntegerPropertyValueEditor(
            Mock.Of<IShortStringHelper>(),
            Mock.Of<IJsonSerializer>(),
            Mock.Of<IIOHelper>(),
            new DataEditorAttribute("alias"),
            localizedTextServiceMock.Object)
        {
            ConfigurationObject = new Dictionary<string, object>
            {
                { "min", 10 },
                { "max", 20 },
                { "step", step }
            }
        };
    }
}
