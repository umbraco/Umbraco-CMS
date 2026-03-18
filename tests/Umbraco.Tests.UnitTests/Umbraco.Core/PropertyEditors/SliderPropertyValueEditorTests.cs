using System.Globalization;
using System.Text.Json.Nodes;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Editors;
using Umbraco.Cms.Core.Models.Validation;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Serialization;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.PropertyEditors;

/// <summary>
/// Tests for the <see cref="SliderPropertyValueEditor"/> class.
/// </summary>
[TestFixture]
public class SliderPropertyValueEditorTests
{
#pragma warning disable IDE1006 // Naming Styles
    public static object[] InvalidCaseData = new object[]
#pragma warning restore IDE1006 // Naming Styles
    {
        123m,
        123,
        -123,
        "something",
        true,
        new object(),
        new List<string> { "some", "values" },
    };

    /// <summary>
    /// Tests that invalid values from the editor are handled correctly and result in null.
    /// </summary>
    /// <param name="value">The value from the editor to test.</param>
    [TestCaseSource(nameof(InvalidCaseData))]
    public void Can_Handle_Invalid_Values_From_Editor(object value)
    {
        var fromEditor = FromEditor(value);
        Assert.IsNull(fromEditor);
    }

    /// <summary>
    /// Tests that the editor can handle invalid Guid values correctly by returning null.
    /// </summary>
    [Test]
    public void Can_Handle_Invalid_Values_From_Editor_Guid()
    {
        var fromEditor = FromEditor(Guid.NewGuid());
        Assert.IsNull(fromEditor);
    }

    /// <summary>
    /// Tests that the editor can handle invalid UDI values gracefully.
    /// </summary>
    [Test]
    public void Can_Handle_Invalid_Values_From_Editor_Udi()
    {
        var fromEditor = FromEditor(new GuidUdi(Constants.UdiEntityType.Document, Guid.NewGuid()));
        Assert.IsNull(fromEditor);
    }

    [TestCase("1", 1)]
    [TestCase("0", 0)]
    [TestCase("-1", -1)]
    [TestCase("123456789", 123456789)]
    [TestCase("123.45", 123.45)]
    public void Can_Parse_Single_Value_To_Editor(string value, decimal expected)
    {
        var toEditor = ToEditor(value) as SliderPropertyEditor.SliderPropertyValueEditor.SliderRange;
        Assert.IsNotNull(toEditor);
        Assert.AreEqual(toEditor.From, expected);
        Assert.AreEqual(toEditor.To, expected);
    }

    /// <summary>
    /// Verifies that a range value string can be correctly parsed by the editor into a <see cref="SliderPropertyEditor.SliderPropertyValueEditor.SliderRange"/> object with the expected From and To values.
    /// </summary>
    /// <param name="value">The input string representing the range, typically in the format "from,to".</param>
    /// <param name="expectedFrom">The expected decimal value for the start of the range (From).</param>
    /// <param name="expectedTo">The expected decimal value for the end of the range (To).</param>
    /// <remarks>
    /// This test ensures that the parsing logic in <see cref="ToEditor"/> correctly interprets range strings and assigns the appropriate values to the resulting SliderRange object.
    /// </remarks>
    [TestCase("1,1", 1, 1)]
    [TestCase("0,0", 0, 0)]
    [TestCase("-1,-1", -1, -1)]
    [TestCase("10,123456789", 10, 123456789)]
    [TestCase("1.234,56", 1.234, 56)]
    [TestCase("4,6.234", 4, 6.234)]
    [TestCase("10.45,15.3", 10.45, 15.3)]
    public void Can_Parse_Range_Value_To_Editor(string value, decimal expectedFrom, decimal expectedTo)
    {
        var toEditor = ToEditor(value) as SliderPropertyEditor.SliderPropertyValueEditor.SliderRange;
        Assert.IsNotNull(toEditor);
        Assert.AreEqual(toEditor.From, expectedFrom);
        Assert.AreEqual(toEditor.To, expectedTo);
    }

    /// <summary>
    /// Tests that valid slider values can be parsed correctly from the editor JSON input.
    /// </summary>
    /// <param name="from">The starting value of the slider range.</param>
    /// <param name="to">The ending value of the slider range.</param>
    /// <param name="expectedResult">The expected string representation of the slider values.</param>
    [TestCase(0, 10, "0,10")]
    [TestCase(10, 10, "10")]
    [TestCase(0, 0, "0")]
    [TestCase(-10, -10, "-10")]
    [TestCase(10, 123456789, "10,123456789")]
    [TestCase(1.5, 1.5, "1.5")]
    [TestCase(0, 0.5, "0,0.5")]
    [TestCase(5, 5.4, "5,5.4")]
    [TestCase(0.5, 0.6, "0.5,0.6")]
    public void Can_Parse_Valid_Value_From_Editor(decimal from, decimal to, string expectedResult)
    {
        var value = JsonNode.Parse($"{{\"from\": {from}, \"to\": {to}}}");
        var fromEditor = FromEditor(value) as string;
        Assert.AreEqual(expectedResult, fromEditor);
    }

    /// <summary>
    /// Tests that invalid values are handled correctly when converting to the editor format.
    /// </summary>
    /// <param name="value">The value to test conversion from.</param>
    [TestCaseSource(nameof(InvalidCaseData))]
    public void Can_Handle_Invalid_Values_To_Editor(object value)
    {
        var toEditor = ToEditor(value);
        Assert.IsNull(toEditor, message: $"Failed for: {value}");
    }

    /// <summary>
    /// Tests that passing null to FromEditor returns null.
    /// </summary>
    [Test]
    public void Null_From_Editor_Yields_Null()
    {
        var result = FromEditor(null);
        Assert.IsNull(result);
    }

    /// <summary>
    /// Tests that passing null to the ToEditor method returns null.
    /// </summary>
    [Test]
    public void Null_To_Editor_Yields_Null()
    {
        var result = ToEditor(null);
        Assert.IsNull(result);
    }

    /// <summary>
    /// Validates that the range is contained only when the range validation is enabled.
    /// </summary>
    /// <param name="enableRange">Indicates whether range validation is enabled.</param>
    /// <param name="from">The start value of the range.</param>
    /// <param name="to">The end value of the range.</param>
    /// <param name="expectedSuccess">Indicates whether the validation is expected to succeed.</param>
    [TestCase(true, 1.1, 1.1, true)]
    [TestCase(true, 1.1, 1.3, true)]
    [TestCase(false, 1.1, 1.1, true)]
    [TestCase(false, 1.1, 1.3, false)]
    public void Validates_Contains_Range_Only_When_Enabled(bool enableRange, decimal from, decimal to, bool expectedSuccess)
    {
        var value = new JsonObject
        {
            { "from", from },
            { "to", to },
        };
        var editor = CreateValueEditor(enableRange: enableRange);
        var result = editor.Validate(value, false, null, PropertyValidationContext.Empty());
        if (expectedSuccess)
        {
            Assert.IsEmpty(result);
        }
        else
        {
            Assert.AreEqual(1, result.Count());

            var validationResult = result.First();
            Assert.AreEqual("validation_unexpectedRange", validationResult.ErrorMessage);
        }
    }

    /// <summary>
    /// Validates that the range defined by the "from" and "to" values is valid only when the validation is enabled.
    /// </summary>
    /// <param name="from">The start value of the range.</param>
    /// <param name="to">The end value of the range.</param>
    /// <param name="expectedSuccess">Indicates whether the validation is expected to succeed.</param>
    [TestCase(1.1, 1.1, true)]
    [TestCase(1.1, 1.3, true)]
    [TestCase(1.3, 1.1, false)]
    public void Validates_Contains_Valid_Range_Only_When_Enabled(decimal from, decimal to, bool expectedSuccess)
    {
        var value = new JsonObject
        {
            { "from", from },
            { "to", to },
        };
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
            Assert.AreEqual("validation_invalidRange", validationResult.ErrorMessage);
        }
    }

    /// <summary>
    /// Validates that the "from" value is greater than or equal to the configured minimum value.
    /// </summary>
    /// <param name="from">The starting value to validate.</param>
    /// <param name="to">The ending value to validate.</param>
    /// <param name="expectedSuccess">Indicates whether the validation is expected to succeed.</param>
    [TestCase(0.9, 1.1, false)]
    [TestCase(1.1, 1.1, true)]
    [TestCase(1.3, 1.7, true)]
    public void Validates_Is_Greater_Than_Or_Equal_To_Configured_Min(decimal from, decimal to, bool expectedSuccess)
    {
        var value = new JsonObject
        {
            { "from", from },
            { "to", to },
        };
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
    /// Validates that the "to" value is less than or equal to the configured maximum value.
    /// </summary>
    /// <param name="from">The starting value of the range.</param>
    /// <param name="to">The ending value of the range to validate against the maximum.</param>
    /// <param name="expectedSuccess">Indicates whether the validation is expected to succeed.</param>
    [TestCase(1.3, 1.7, true)]
    [TestCase(1.9, 1.9, true)]
    [TestCase(1.9, 2.1, false)]
    public void Validates_Is_Less_Than_Or_Equal_To_Configured_Max(decimal from, decimal to, bool expectedSuccess)
    {
        var value = new JsonObject
        {
            { "from", from },
            { "to", to },
        };
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
    /// Tests that the max item validation treats 0 as unlimited.
    /// </summary>
    [Test]
    public void Max_Item_Validation_Respects_0_As_Unlimited()
    {
        var value = new JsonObject
        {
            { "from", 1.0m },
            { "to", 1.0m },
        };
        var editor = CreateValueEditor();
        editor.ConfigurationObject = new SliderConfiguration();

        var result = editor.Validate(value, false, null, PropertyValidationContext.Empty());
        Assert.IsEmpty(result);
    }

    /// <summary>
    /// Tests that the slider property value is validated against the configured step increment.
    /// Ensures that the difference between the 'from' and 'to' values is a multiple of the specified step, or that validation passes when the step is zero.
    /// </summary>
    /// <param name="step">The step increment configured for the slider; if zero, validation always passes.</param>
    /// <param name="from">The starting value of the slider range to validate.</param>
    /// <param name="to">The ending value of the slider range to validate.</param>
    /// <param name="expectedSuccess">True if the validation is expected to succeed; otherwise, false.</param>
    [TestCase(0.2, 1.3, 1.7, true)]
    [TestCase(0.2, 1.4, 1.7, false)]
    [TestCase(0.2, 1.3, 1.6, false)]
    [TestCase(0.0, 1.4, 1.7, true)] // A step of zero would trigger a divide by zero error in evaluating. So we always pass validation for zero, as effectively any step value is valid.
    public void Validates_Matches_Configured_Step(decimal step, decimal from, decimal to, bool expectedSuccess)
    {
        var value = new JsonObject
        {
            { "from", from },
            { "to", to },
        };
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

    private static SliderPropertyEditor.SliderPropertyValueEditor CreateValueEditor(bool enableRange = true, decimal step = 0.2m)
    {
        var localizedTextServiceMock = new Mock<ILocalizedTextService>();
        localizedTextServiceMock.Setup(x => x.Localize(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CultureInfo>(),
                It.IsAny<IDictionary<string, string>>()))
            .Returns((string key, string alias, CultureInfo culture, IDictionary<string, string> args) => $"{key}_{alias}");
        return new SliderPropertyEditor.SliderPropertyValueEditor(
            Mock.Of<IShortStringHelper>(),
            new SystemTextJsonSerializer(new DefaultJsonSerializerEncoderFactory()),
            Mock.Of<IIOHelper>(),
            new DataEditorAttribute("alias"),
            localizedTextServiceMock.Object)
        {
            ConfigurationObject = new SliderConfiguration
            {
                EnableRange = enableRange,
                MinimumValue = 1.1m,
                MaximumValue = 1.9m,
                Step = step
            },
        };
    }
}
