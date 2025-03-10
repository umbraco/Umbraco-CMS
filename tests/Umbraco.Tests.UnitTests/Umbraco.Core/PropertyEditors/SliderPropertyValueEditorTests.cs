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
        Guid.NewGuid(),
        new GuidUdi(Constants.UdiEntityType.Document, Guid.NewGuid())
    };

    [TestCaseSource(nameof(InvalidCaseData))]
    public void Can_Handle_Invalid_Values_From_Editor(object value)
    {
        var fromEditor = FromEditor(value);
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

    [TestCaseSource(nameof(InvalidCaseData))]
    public void Can_Handle_Invalid_Values_To_Editor(object value)
    {
        var toEditor = ToEditor(value);
        Assert.IsNull(toEditor, message: $"Failed for: {value}");
    }

    [Test]
    public void Null_From_Editor_Yields_Null()
    {
        var result = FromEditor(null);
        Assert.IsNull(result);
    }

    [Test]
    public void Null_To_Editor_Yields_Null()
    {
        var result = ToEditor(null);
        Assert.IsNull(result);
    }

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
            new SystemTextJsonSerializer(),
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
