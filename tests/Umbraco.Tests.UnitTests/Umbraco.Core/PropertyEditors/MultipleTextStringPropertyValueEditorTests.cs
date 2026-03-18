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
/// Contains unit tests for the <see cref="MultipleTextStringPropertyValueEditor"/> class.
/// </summary>
[TestFixture]
public class MultipleTextStringPropertyValueEditorTests
{
    /// <summary>
    /// Tests that the MultipleTextStringPropertyValueEditor correctly handles invalid values from the editor by returning null.
    /// </summary>
    [Test]
    public void Can_Handle_Invalid_Values_From_Editor()
    {
        // annoyingly we can't use decimals etc. in attributes, so we can't turn these into test cases :(
        var invalidValues = new List<object?>
        {
            123m,
            123,
            -123,
            123.45d,
            "123.45",
            "1.234,56",
            "1.2.3.4",
            "something",
            true,
            new object(),
            Guid.NewGuid(),
            new GuidUdi(Constants.UdiEntityType.Document, Guid.NewGuid())
        };

        foreach (var value in invalidValues)
        {
            var fromEditor = FromEditor(value);
            Assert.IsNull(fromEditor, message: $"Failed for: {value}");
        }
    }

    /// <summary>
    /// Tests that the editor can handle invalid values gracefully by returning an empty enumerable.
    /// </summary>
    [Test]
    public void Can_Handle_Invalid_Values_To_Editor()
    {
        // annoyingly we can't use decimals etc. in attributes, so we can't turn these into test cases :(
        var invalidValues = new List<object?>
        {
            123m,
            123,
            -123,
            123.45d,
            true,
            new object(),
            Guid.NewGuid(),
            new GuidUdi(Constants.UdiEntityType.Document, Guid.NewGuid())
        };

        foreach (var value in invalidValues)
        {
            var toEditor = ToEditor(value) as IEnumerable<string>;
            Assert.IsNotNull(toEditor, message: $"Failed for: {value}");
            Assert.IsEmpty(toEditor, message: $"Failed for: {value}");
        }
    }

    /// <summary>
    /// Tests that a single value can be parsed correctly from the editor.
    /// </summary>
    [Test]
    public void Can_Parse_Single_Value_From_Editor()
    {
        var fromEditor = FromEditor(new[] { "The Value" }) as string;
        Assert.AreEqual("The Value", fromEditor);
    }

    /// <summary>
    /// Tests that multiple values can be parsed correctly from the editor input.
    /// </summary>
    [Test]
    public void Can_Parse_Multi_Value_From_Editor()
    {
        var fromEditor = FromEditor(new[] { "The First Value", "The Second Value", "The Third Value" }) as string;
        Assert.AreEqual("The First Value\nThe Second Value\nThe Third Value", fromEditor);
    }

    /// <summary>
    /// Tests that the value editor can parse more items than the allowed maximum from the editor input.
    /// It also verifies that the validation correctly identifies when the number of items exceeds the maximum allowed.
    /// </summary>
    [Test]
    public void Can_Parse_More_Items_Than_Allowed_From_Editor()
    {
        var valueEditor = CreateValueEditor();
        var fromEditor = valueEditor.FromEditor(new ContentPropertyData(new[] { "One", "Two", "Three", "Four", "Five" }, new MultipleTextStringConfiguration { Max = 4 }), null) as string;
        Assert.AreEqual("One\nTwo\nThree\nFour\nFive", fromEditor);

        var validationResults = valueEditor.Validate(fromEditor, false, null, PropertyValidationContext.Empty());
        Assert.AreEqual(1, validationResults.Count());

        var validationResult = validationResults.First();
        Assert.AreEqual($"validation_outOfRangeMultipleItemsMaximum", validationResult.ErrorMessage);
    }

    /// <summary>
    /// Tests that a single string value can be parsed correctly to the editor format.
    /// </summary>
    [Test]
    public void Can_Parse_Single_Value_To_Editor()
    {
        var toEditor = ToEditor("The Value") as IEnumerable<string>;
        Assert.IsNotNull(toEditor);
        Assert.AreEqual(1, toEditor.Count());
        Assert.AreEqual("The Value", toEditor.First());
    }

    /// <summary>
    /// Tests that multiple values separated by new lines can be parsed correctly to the editor format.
    /// </summary>
    [Test]
    public void Can_Parse_Multi_Value_To_Editor()
    {
        var toEditor = ToEditor("The First Value\nThe Second Value\nThe Third Value") as IEnumerable<string>;
        Assert.IsNotNull(toEditor);
        Assert.AreEqual(3, toEditor.Count());
        Assert.AreEqual("The First Value", toEditor.First());
        Assert.AreEqual("The Second Value", toEditor.Skip(1).First());
        Assert.AreEqual("The Third Value", toEditor.Last());
    }

    /// <summary>
    /// Tests that passing null from the editor results in a null value.
    /// </summary>
    [Test]
    public void Null_From_Editor_Yields_Null()
    {
        var result = FromEditor(null);
        Assert.IsNull(result);
    }

    /// <summary>
    /// Tests that converting a null value to editor format yields an empty collection.
    /// </summary>
    [Test]
    public void Null_To_Editor_Yields_Empty_Collection()
    {
        var result = ToEditor(null) as IEnumerable<string>;
        Assert.IsNotNull(result);
        Assert.IsEmpty(result);
    }

    /// <summary>
    /// Tests that the value editor validates null as below the configured minimum.
    /// </summary>
    [Test]
    public void Validates_Null_As_Below_Configured_Min()
    {
        var editor = CreateValueEditor();
        var result = editor.Validate(null, false, null, PropertyValidationContext.Empty());
        Assert.AreEqual(1, result.Count());

        var validationResult = result.First();
        Assert.AreEqual($"validation_outOfRangeMultipleItemsMinimum", validationResult.ErrorMessage);
    }

    [TestCase(0, false, "outOfRangeMultipleItemsMinimum")]
    [TestCase(1, false, "outOfRangeSingleItemMinimum")]
    [TestCase(2, true, "")]
    [TestCase(3, true, "")]
    public void Validates_Number_Of_Items_Is_Greater_Than_Or_Equal_To_Configured_Min(int numberOfStrings, bool expectedSuccess, string expectedValidationMessageKey)
    {
        var value = Enumerable.Range(1, numberOfStrings).Select(x => x.ToString());
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
            Assert.AreEqual($"validation_{expectedValidationMessageKey}", validationResult.ErrorMessage);
        }
    }

    /// <summary>
    /// Tests that the value editor correctly validates whether the number of items in the raw property value
    /// (separated by new lines) meets or exceeds the configured minimum requirement.
    /// </summary>
    /// <param name="value">The raw string value to validate, containing one or more items separated by new lines.</param>
    /// <param name="expectedSuccess">True if the validation is expected to pass; otherwise, false.</param>
    /// <param name="expectedValidationMessageKey">The expected validation message key if validation fails; otherwise, an empty string if validation succeeds.</param>
    [TestCase("", false, "outOfRangeMultipleItemsMinimum")]
    [TestCase("one", false, "outOfRangeSingleItemMinimum")]
    [TestCase("one\ntwo", true, "")]
    [TestCase("one\ntwo\nthree", true, "")]
    public void Validates_Number_Of_Items_Is_Greater_Than_Or_Equal_To_Configured_Min_Raw_Property_Value(string value, bool expectedSuccess, string expectedValidationMessageKey)
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
            Assert.AreEqual($"validation_{expectedValidationMessageKey}", validationResult.ErrorMessage);
        }
    }

    /// <summary>
    /// Validates that the number of items is less than or equal to the configured maximum.
    /// </summary>
    /// <param name="numberOfStrings">The number of string items to validate.</param>
    /// <param name="expectedSuccess">Indicates whether the validation is expected to succeed.</param>
    [TestCase(3, true)]
    [TestCase(4, true)]
    [TestCase(5, false)]
    public void Validates_Number_Of_Items_Is_Less_Than_Or_Equal_To_Configured_Max(int numberOfStrings, bool expectedSuccess)
    {
        var value = Enumerable.Range(1, numberOfStrings).Select(x => x.ToString());
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
            Assert.AreEqual("validation_outOfRangeMultipleItemsMaximum", validationResult.ErrorMessage);
        }
    }

    [TestCase("one\ntwo\nthree", true)]
    [TestCase("one\ntwo\nthree\nfour", true)]
    [TestCase("one\ntwo\nthree\nfour\nfive", false)]
    public void Validates_Number_Of_Items_Is_Less_Than_Or_Equal_To_Configured_Max_Raw_Property_Value(string value, bool expectedSuccess)
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
            Assert.AreEqual("validation_outOfRangeMultipleItemsMaximum", validationResult.ErrorMessage);
        }
    }

    /// <summary>
    /// Tests that the property value editor can parse supported property value delimiters correctly.
    /// </summary>
    /// <param name="value">The string value containing multiple text entries separated by delimiters.</param>
    [TestCase("one\ntwo\nthree")]
    [TestCase("one\rtwo\rthree")]
    [TestCase("one\r\ntwo\r\nthree")]
    public void Can_Parse_Supported_Property_Value_Delimiters(string value)
    {
        var editor = CreateValueEditor();
        var result = editor.Validate(value, false, null, PropertyValidationContext.Empty());
        Assert.IsEmpty(result);
    }

    /// <summary>
    /// Tests that the maximum item validation treats 0 as unlimited.
    /// </summary>
    [Test]
    public void Max_Item_Validation_Respects_0_As_Unlimited()
    {
        var value = Enumerable.Range(1, 100).Select(x => x.ToString());
        var editor = CreateValueEditor();
        editor.ConfigurationObject = new MultipleTextStringConfiguration();

        var result = editor.Validate(value, false, null, PropertyValidationContext.Empty());
        Assert.IsEmpty(result);
    }

    /// <summary>
    /// Tests that FromEditor filters out empty and whitespace strings correctly.
    /// </summary>
    [Test]
    public void FromEditor_Filters_Empty_Strings()
    {
        var fromEditor = FromEditor(new[] { "one", string.Empty, "two", "  ", "three" }) as string;
        Assert.AreEqual("one\ntwo\nthree", fromEditor);
    }

    /// <summary>
    /// Tests that FromEditor returns null when given an empty collection.
    /// </summary>
    [Test]
    public void FromEditor_Returns_Null_For_Empty_Collection()
    {
        var fromEditor = FromEditor(Array.Empty<string>());
        Assert.IsNull(fromEditor);
    }

    /// <summary>
    /// Tests that FromEditor returns null when all input strings are empty or whitespace.
    /// </summary>
    [Test]
    public void FromEditor_Returns_Null_When_All_Strings_Are_Empty()
    {
        var fromEditor = FromEditor(new[] { string.Empty, "  ", "\t" });
        Assert.IsNull(fromEditor);
    }

    /// <summary>
    /// Tests that FromEditor preserves non-empty strings when mixed with empty strings.
    /// </summary>
    [Test]
    public void FromEditor_Preserves_Non_Empty_Strings_Mixed_With_Empty()
    {
        var fromEditor = FromEditor(new[] { string.Empty, "valid@email.com", string.Empty }) as string;
        Assert.AreEqual("valid@email.com", fromEditor);
    }

    /// <summary>
    /// Tests that the format validator skips empty strings and does not fail validation on them.
    /// </summary>
    [Test]
    public void Format_Validator_Skips_Empty_Strings()
    {
        var editor = CreateValueEditor();
        editor.ConfigurationObject = new MultipleTextStringConfiguration();

        // Email regex pattern - empty strings should be skipped, not fail validation.
        const string emailRegex = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
        var value = new[] { string.Empty, "valid@email.com" };

        var result = editor.Validate(value, false, emailRegex, PropertyValidationContext.Empty());
        Assert.IsEmpty(result);
    }

    /// <summary>
    /// Tests that the format validator fails for invalid non-empty strings.
    /// </summary>
    [Test]
    public void Format_Validator_Fails_For_Invalid_Non_Empty_Strings()
    {
        var editor = CreateValueEditor();
        editor.ConfigurationObject = new MultipleTextStringConfiguration();

        const string emailRegex = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
        var value = new[] { "not-an-email", "valid@email.com" };

        var result = editor.Validate(value, false, emailRegex, PropertyValidationContext.Empty());
        Assert.IsNotEmpty(result);
    }

    /// <summary>
    /// Tests that the MinMax validator does not count empty strings when validating the value.
    /// </summary>
    [Test]
    public void MinMax_Validator_Does_Not_Count_Empty_Strings()
    {
        var editor = CreateValueEditor();
        editor.ConfigurationObject = new MultipleTextStringConfiguration { Min = 2, Max = 4 };

        // 2 non-empty + 2 empty = should count as 2, meeting min=2
        var value = new[] { "one", string.Empty, "two", string.Empty };
        var result = editor.Validate(value, false, null, PropertyValidationContext.Empty());
        Assert.IsEmpty(result);
    }

    /// <summary>
    /// Tests that the MinMax validator does not count empty strings when validating against the minimum count.
    /// </summary>
    [Test]
    public void MinMax_Validator_Does_Not_Count_Empty_Strings_Below_Min()
    {
        var editor = CreateValueEditor();
        editor.ConfigurationObject = new MultipleTextStringConfiguration { Min = 2, Max = 4 };

        // 1 non-empty + 2 empty = should count as 1, failing min=2
        var value = new[] { "one", string.Empty, string.Empty };
        var result = editor.Validate(value, false, null, PropertyValidationContext.Empty());
        Assert.AreEqual(1, result.Count());
        Assert.AreEqual("validation_outOfRangeSingleItemMinimum", result.First().ErrorMessage);
    }

    /// <summary>
    /// Tests that the required validator fails when all strings in the value array are empty or whitespace.
    /// </summary>
    [Test]
    public void Required_Validator_Fails_When_All_Strings_Are_Empty()
    {
        var editor = CreateValueEditor();
        editor.ConfigurationObject = new MultipleTextStringConfiguration();

        var value = new[] { string.Empty, "  " };
        var result = editor.Validate(value, true, null, PropertyValidationContext.Empty());
        Assert.IsNotEmpty(result);
    }

    /// <summary>
    /// Tests that the required validator passes when at least one non-empty string is present in the value.
    /// </summary>
    [Test]
    public void Required_Validator_Passes_When_Non_Empty_Strings_Present()
    {
        var editor = CreateValueEditor();
        editor.ConfigurationObject = new MultipleTextStringConfiguration();

        var value = new[] { string.Empty, "valid@email.com" };
        var result = editor.Validate(value, true, null, PropertyValidationContext.Empty());
        Assert.IsEmpty(result);
    }

    private static object? FromEditor(object? value, int max = 0)
        => CreateValueEditor().FromEditor(new ContentPropertyData(value, new MultipleTextStringConfiguration { Max = max }), null);

    private static object? ToEditor(object? value)
    {
        var property = new Mock<IProperty>();
        property
            .Setup(p => p.GetValue(It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<bool>()))
            .Returns(value);

        return CreateValueEditor().ToEditor(property.Object);
    }

    private static MultipleTextStringPropertyEditor.MultipleTextStringPropertyValueEditor CreateValueEditor()
    {
        var localizedTextServiceMock = new Mock<ILocalizedTextService>();
        localizedTextServiceMock.Setup(x => x.Localize(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CultureInfo>(),
                It.IsAny<IDictionary<string, string>>()))
            .Returns((string key, string alias, CultureInfo culture, IDictionary<string, string> args) => $"{key}_{alias}");
        return new MultipleTextStringPropertyEditor.MultipleTextStringPropertyValueEditor(
            Mock.Of<IShortStringHelper>(),
            Mock.Of<IJsonSerializer>(),
            Mock.Of<IIOHelper>(),
            new DataEditorAttribute("alias"),
            localizedTextServiceMock.Object)
        {
            ConfigurationObject = new MultipleTextStringConfiguration
            {
                Min = 2,
                Max = 4,
            },
        };
    }
}
