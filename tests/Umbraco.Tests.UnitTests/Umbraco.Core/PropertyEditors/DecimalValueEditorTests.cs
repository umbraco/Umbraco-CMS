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

[TestFixture]
public class DecimalValueEditorTests
{
    // annoyingly we can't use decimals etc. in attributes, so we can't turn these into test cases :(
    private Dictionary<object?,object?> _valuesAndExpectedResults = new();

    [SetUp]
    public void SetUp() => _valuesAndExpectedResults = new Dictionary<object?, object?>
    {
        { 123m, 123m },
        { 123, 123m },
        { -123, -123m },
        { 123.45d, 123.45m },
        { "123.45", 123.45m },
        { "1234.56", 1234.56m },
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

    [Test]
    public void Can_Parse_Values_From_Editor()
    {
        foreach (var (value, expected) in _valuesAndExpectedResults)
        {
            var fromEditor = FromEditor(value);
            Assert.AreEqual(expected, fromEditor, message: $"Failed for: {value}");
        }
    }

    [Test]
    public void Can_Parse_Values_To_Editor()
    {
        foreach (var (value, expected) in _valuesAndExpectedResults)
        {
            var toEditor = ToEditor(value);
            Assert.AreEqual(expected, toEditor, message: $"Failed for: {value}");
        }
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
            Assert.AreEqual(validationResult.ErrorMessage, $"The value {value} is not a valid decimal");
        }
    }

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
            Assert.AreEqual(validationResult.ErrorMessage, "validation_outOfRangeMinimum");
        }
    }

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
            Assert.AreEqual(validationResult.ErrorMessage, "validation_outOfRangeMaximum");
        }
    }

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
            Assert.AreEqual(validationResult.ErrorMessage, "validation_invalidStep");
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

    private static DecimalPropertyEditor.DecimalPropertyValueEditor CreateValueEditor(double step = 0.2)
    {
        var localizedTextServiceMock = new Mock<ILocalizedTextService>();
        localizedTextServiceMock.Setup(x => x.Localize(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CultureInfo>(),
                It.IsAny<IDictionary<string, string>>()))
            .Returns((string key, string alias, CultureInfo culture, IDictionary<string, string> args) => $"{key}_{alias}");
        return new DecimalPropertyEditor.DecimalPropertyValueEditor(
            Mock.Of<IShortStringHelper>(),
            Mock.Of<IJsonSerializer>(),
            Mock.Of<IIOHelper>(),
            new DataEditorAttribute("alias"),
            localizedTextServiceMock.Object)
        {
            ConfigurationObject = new Dictionary<string, object>
            {
                { "min", 1.1 },
                { "max", 1.9 },
                { "step", step }
            }
        };
    }
}
