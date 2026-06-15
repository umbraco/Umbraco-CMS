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
public class MultipleTextStringPropertyValueEditorTests
{
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
            Assert.That(fromEditor, Is.Null, message: $"Failed for: {value}");
        }
    }

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
            Assert.That(toEditor, Is.Not.Null, message: $"Failed for: {value}");
            Assert.That(toEditor, Is.Empty, message: $"Failed for: {value}");
        }
    }

    [Test]
    public void Can_Parse_Single_Value_From_Editor()
    {
        var fromEditor = FromEditor(new[] { "The Value" }) as string;
        Assert.That(fromEditor, Is.EqualTo("The Value"));
    }

    [Test]
    public void Can_Parse_Multi_Value_From_Editor()
    {
        var fromEditor = FromEditor(new[] { "The First Value", "The Second Value", "The Third Value" }) as string;
        Assert.That(fromEditor, Is.EqualTo("The First Value\nThe Second Value\nThe Third Value"));
    }

    [Test]
    public void Can_Parse_More_Items_Than_Allowed_From_Editor()
    {
        var valueEditor = CreateValueEditor();
        var fromEditor = valueEditor.FromEditor(new ContentPropertyData(new[] { "One", "Two", "Three", "Four", "Five" }, new MultipleTextStringConfiguration { Max = 4 }), null) as string;
        Assert.That(fromEditor, Is.EqualTo("One\nTwo\nThree\nFour\nFive"));

        var validationResults = valueEditor.Validate(fromEditor, false, null, PropertyValidationContext.Empty());
        Assert.That(validationResults.Count(), Is.EqualTo(1));

        var validationResult = validationResults.First();
        Assert.That(validationResult.ErrorMessage, Is.EqualTo($"validation_outOfRangeMultipleItemsMaximum"));
    }

    [Test]
    public void Can_Parse_Single_Value_To_Editor()
    {
        var toEditor = ToEditor("The Value") as IEnumerable<string>;
        Assert.That(toEditor, Is.Not.Null);
        Assert.That(toEditor.Count(), Is.EqualTo(1));
        Assert.That(toEditor.First(), Is.EqualTo("The Value"));
    }

    [Test]
    public void Can_Parse_Multi_Value_To_Editor()
    {
        var toEditor = ToEditor("The First Value\nThe Second Value\nThe Third Value") as IEnumerable<string>;
        Assert.That(toEditor, Is.Not.Null);
        Assert.That(toEditor.Count(), Is.EqualTo(3));
        Assert.That(toEditor.First(), Is.EqualTo("The First Value"));
        Assert.That(toEditor.Skip(1).First(), Is.EqualTo("The Second Value"));
        Assert.That(toEditor.Last(), Is.EqualTo("The Third Value"));
    }

    [Test]
    public void Null_From_Editor_Yields_Null()
    {
        var result = FromEditor(null);
        Assert.That(result, Is.Null);
    }

    [Test]
    public void Null_To_Editor_Yields_Empty_Collection()
    {
        var result = ToEditor(null) as IEnumerable<string>;
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.Empty);
    }

    [Test]
    public void Validates_Null_As_Below_Configured_Min()
    {
        var editor = CreateValueEditor();
        var result = editor.Validate(null, false, null, PropertyValidationContext.Empty());
        Assert.That(result.Count(), Is.EqualTo(1));

        var validationResult = result.First();
        Assert.That(validationResult.ErrorMessage, Is.EqualTo($"validation_outOfRangeMultipleItemsMinimum"));
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
            Assert.That(result, Is.Empty);
        }
        else
        {
            Assert.That(result.Count(), Is.EqualTo(1));

            var validationResult = result.First();
            Assert.That(validationResult.ErrorMessage, Is.EqualTo($"validation_{expectedValidationMessageKey}"));
        }
    }

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
            Assert.That(result, Is.Empty);
        }
        else
        {
            Assert.That(result.Count(), Is.EqualTo(1));

            var validationResult = result.First();
            Assert.That(validationResult.ErrorMessage, Is.EqualTo($"validation_{expectedValidationMessageKey}"));
        }
    }

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
            Assert.That(result, Is.Empty);
        }
        else
        {
            Assert.That(result.Count(), Is.EqualTo(1));

            var validationResult = result.First();
            Assert.That(validationResult.ErrorMessage, Is.EqualTo("validation_outOfRangeMultipleItemsMaximum"));
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
            Assert.That(result, Is.Empty);
        }
        else
        {
            Assert.That(result.Count(), Is.EqualTo(1));

            var validationResult = result.First();
            Assert.That(validationResult.ErrorMessage, Is.EqualTo("validation_outOfRangeMultipleItemsMaximum"));
        }
    }

    [TestCase("one\ntwo\nthree")]
    [TestCase("one\rtwo\rthree")]
    [TestCase("one\r\ntwo\r\nthree")]
    public void Can_Parse_Supported_Property_Value_Delimiters(string value)
    {
        var editor = CreateValueEditor();
        var result = editor.Validate(value, false, null, PropertyValidationContext.Empty());
        Assert.That(result, Is.Empty);
    }

    [Test]
    public void Max_Item_Validation_Respects_0_As_Unlimited()
    {
        var value = Enumerable.Range(1, 100).Select(x => x.ToString());
        var editor = CreateValueEditor();
        editor.ConfigurationObject = new MultipleTextStringConfiguration();

        var result = editor.Validate(value, false, null, PropertyValidationContext.Empty());
        Assert.That(result, Is.Empty);
    }

    [Test]
    public void FromEditor_Filters_Empty_Strings()
    {
        var fromEditor = FromEditor(new[] { "one", string.Empty, "two", "  ", "three" }) as string;
        Assert.That(fromEditor, Is.EqualTo("one\ntwo\nthree"));
    }

    [Test]
    public void FromEditor_Returns_Null_For_Empty_Collection()
    {
        var fromEditor = FromEditor(Array.Empty<string>());
        Assert.That(fromEditor, Is.Null);
    }

    [Test]
    public void FromEditor_Returns_Null_When_All_Strings_Are_Empty()
    {
        var fromEditor = FromEditor(new[] { string.Empty, "  ", "\t" });
        Assert.That(fromEditor, Is.Null);
    }

    [Test]
    public void FromEditor_Preserves_Non_Empty_Strings_Mixed_With_Empty()
    {
        var fromEditor = FromEditor(new[] { string.Empty, "valid@email.com", string.Empty }) as string;
        Assert.That(fromEditor, Is.EqualTo("valid@email.com"));
    }

    [Test]
    public void Format_Validator_Skips_Empty_Strings()
    {
        var editor = CreateValueEditor();
        editor.ConfigurationObject = new MultipleTextStringConfiguration();

        // Email regex pattern - empty strings should be skipped, not fail validation.
        const string emailRegex = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
        var value = new[] { string.Empty, "valid@email.com" };

        var result = editor.Validate(value, false, emailRegex, PropertyValidationContext.Empty());
        Assert.That(result, Is.Empty);
    }

    [Test]
    public void Format_Validator_Fails_For_Invalid_Non_Empty_Strings()
    {
        var editor = CreateValueEditor();
        editor.ConfigurationObject = new MultipleTextStringConfiguration();

        const string emailRegex = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
        var value = new[] { "not-an-email", "valid@email.com" };

        var result = editor.Validate(value, false, emailRegex, PropertyValidationContext.Empty());
        Assert.That(result, Is.Not.Empty);
    }

    [Test]
    public void MinMax_Validator_Does_Not_Count_Empty_Strings()
    {
        var editor = CreateValueEditor();
        editor.ConfigurationObject = new MultipleTextStringConfiguration { Min = 2, Max = 4 };

        // 2 non-empty + 2 empty = should count as 2, meeting min=2
        var value = new[] { "one", string.Empty, "two", string.Empty };
        var result = editor.Validate(value, false, null, PropertyValidationContext.Empty());
        Assert.That(result, Is.Empty);
    }

    [Test]
    public void MinMax_Validator_Does_Not_Count_Empty_Strings_Below_Min()
    {
        var editor = CreateValueEditor();
        editor.ConfigurationObject = new MultipleTextStringConfiguration { Min = 2, Max = 4 };

        // 1 non-empty + 2 empty = should count as 1, failing min=2
        var value = new[] { "one", string.Empty, string.Empty };
        var result = editor.Validate(value, false, null, PropertyValidationContext.Empty());
        Assert.That(result.Count(), Is.EqualTo(1));
        Assert.That(result.First().ErrorMessage, Is.EqualTo("validation_outOfRangeSingleItemMinimum"));
    }

    [Test]
    public void Required_Validator_Fails_When_All_Strings_Are_Empty()
    {
        var editor = CreateValueEditor();
        editor.ConfigurationObject = new MultipleTextStringConfiguration();

        var value = new[] { string.Empty, "  " };
        var result = editor.Validate(value, true, null, PropertyValidationContext.Empty());
        Assert.That(result, Is.Not.Empty);
    }

    [Test]
    public void Required_Validator_Passes_When_Non_Empty_Strings_Present()
    {
        var editor = CreateValueEditor();
        editor.ConfigurationObject = new MultipleTextStringConfiguration();

        var value = new[] { string.Empty, "valid@email.com" };
        var result = editor.Validate(value, true, null, PropertyValidationContext.Empty());
        Assert.That(result, Is.Empty);
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
