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
/// Contains unit tests for <see cref="global::Umbraco.Core.PropertyEditors.DecimalPropertyValueEditor"/>.
/// </summary>
[TestFixture]
public class DecimalPropertyValueEditorTests
{
    // annoyingly we can't use decimals etc. in attributes, so we can't turn these into test cases :(
    private Dictionary<object?,object?> _valuesAndExpectedResults = new();

    /// <summary>
    /// Sets up the test environment by initializing the dictionary of values and their expected decimal results.
    /// </summary>
    [SetUp]
    public void SetUp() => _valuesAndExpectedResults = new Dictionary<object?, object?>
    {
        { 123m, 123m },
        { 123, 123m },
        { -123, -123m },
        { 123.45d, 123.45m },
        { 123.45f, 123.45m },
        { "123.45", 123.45m },
        { "1234.56", 1234.56m },
        { "1,234.56", 1234.56m },
        { "123,45", 12345m },
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
    /// Tests that decimal values can be correctly parsed from the editor input.
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
    /// Verifies that decimal values entered in the editor are parsed correctly when using a culture (such as Italian) that does not use the en-US decimal separator.
    /// </summary>
    /// <param name="value">The input value from the editor, using various decimal and thousands separators.</param>
    /// <param name="expected">The expected decimal result after parsing, or <c>null</c> if parsing should fail.</param>
    /// <remarks>
    /// The test uses the "it-IT" culture to ensure parsing respects non-en-US decimal separators, and covers cases with both valid and invalid formats.
    /// </remarks>
    [SetCulture("it-IT")]
    [SetUICulture("it-IT")]
    [TestCase("123,45", 123.45)]
    [TestCase("1.234,56", 1234.56)]
    [TestCase("123.45", 12345)]
    [TestCase("1,234.56", null)]
    public void Can_Parse_Values_From_Editor_Using_Culture_With_Non_EnUs_Decimal_Separator(object value, decimal? expected)
    {
        var fromEditor = FromEditor(value);
        Assert.AreEqual(expected, fromEditor);
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
    /// Tests that converting a null value to the editor returns null.
    /// </summary>
    [Test]
    public void Null_To_Editor_Yields_Null()
    {
        var result = ToEditor(null);
        Assert.IsNull(result);
    }

    [TestCase("x", false)]
    [TestCase(1.5, true)]
    public void Validates_Is_Decimal(object value, bool expectedSuccess)
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
            Assert.AreEqual($"The value {value} is not a valid decimal", validationResult.ErrorMessage);
        }
    }

    /// <summary>
    /// Tests that the decimal property value editor correctly validates whether a given value
    /// is greater than or equal to the configured minimum value.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <param name="expectedSuccess">True if the value is expected to pass validation; otherwise, false.</param>
    [TestCase(0.9, false)]
    [TestCase(1.1, true)]
    [TestCase(1.3, true)]
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
    /// Tests that the value editor correctly validates whether a provided value is less than or equal to the configured maximum value.
    /// </summary>
    /// <param name="value">The value to validate against the maximum constraint.</param>
    /// <param name="expectedSuccess">True if the value is expected to pass validation; otherwise, false.</param>
    [TestCase(1.7, true)]
    [TestCase(1.9, true)]
    [TestCase(2.1, false)]
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
    /// Tests that the value is validated as less than or equal to the configured maximum value
    /// when the value editor is set up with whole number constraints.
    /// </summary>
    /// <param name="value">The value to validate against the maximum constraint.</param>
    /// <param name="expectedSuccess">True if the value is expected to pass validation; otherwise, false.</param>
    [TestCase(1.8, true)]
    [TestCase(2.2, false)]
    public void Validates_Is_Less_Than_Or_Equal_To_Configured_Max_With_Configured_Whole_Numbers(object value, bool expectedSuccess)
    {
        var editor = CreateValueEditor(min: 1, max: 2);
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
    /// Tests that the value is validated as less than or equal to the configured maximum when using a culture (it-IT) with a comma decimal separator.
    /// </summary>
    /// <param name="value">The value to validate, using a comma as the decimal separator in the current culture.</param>
    /// <param name="expectedSuccess">True if the value is expected to pass validation; otherwise, false.</param>
    [TestCase(1.8, true)]
    [TestCase(2.2, false)]
    [SetCulture("it-IT")] // Uses "," as the decimal separator.
    public void Validates_Is_Less_Than_Or_Equal_To_Configured_Max_With_Comma_Decimal_Separator(object value, bool expectedSuccess)
    {
        var editor = CreateValueEditor(min: 1, max: 2);
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
    /// Validates that the value matches the configured step increment.
    /// </summary>
    /// <param name="step">The step increment to validate against.</param>
    /// <param name="value">The value to validate.</param>
    /// <param name="expectedSuccess">Indicates whether the validation is expected to succeed.</param>
    [TestCase(0.2, 1.4, false)]
    [TestCase(0.2, 1.5, true)]
    [TestCase(0.0, 1.4, true)] // A step of zero would trigger a divide by zero error in evaluating. So we always pass validation for zero, as effectively any step value is valid.
    public void Validates_Matches_Configured_Step(double step, object value, bool expectedSuccess)
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

    private static DecimalPropertyEditor.DecimalPropertyValueEditor CreateValueEditor(double min = 1.1, double max = 1.9, double step = 0.2)
    {
        var localizedTextServiceMock = new Mock<ILocalizedTextService>();
        localizedTextServiceMock.Setup(x => x.Localize(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CultureInfo>(),
                It.IsAny<IDictionary<string, string>>()))
            .Returns((string key, string alias, CultureInfo culture, IDictionary<string, string> args) => $"{key}_{alias}");

        // When configuration is populated from the deserialized JSON, whole number values are deserialized as integers.
        // So we want to replicate that in our tests.
        var configuration = new Dictionary<string, object>();
        if (min % 1 == 0)
        {
            configuration.Add("min", (int)min);
        }
        else
        {
            configuration.Add("min", min);
        }

        if (max % 1 == 0)
        {
            configuration.Add("max", (int)max);
        }
        else
        {
            configuration.Add("max", max);
        }

        if (step % 1 == 0)
        {
            configuration.Add("step", (int)step);
        }
        else
        {
            configuration.Add("step", step);
        }

        return new DecimalPropertyEditor.DecimalPropertyValueEditor(
            Mock.Of<IShortStringHelper>(),
            Mock.Of<IJsonSerializer>(),
            Mock.Of<IIOHelper>(),
            new DataEditorAttribute("alias"),
            localizedTextServiceMock.Object)
        {
            ConfigurationObject = configuration
        };
    }
}
