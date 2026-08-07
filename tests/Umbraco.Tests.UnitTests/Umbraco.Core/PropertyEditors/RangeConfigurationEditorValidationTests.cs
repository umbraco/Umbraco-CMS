using System.Text.Json.Nodes;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.PropertyEditors;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.PropertyEditors;

/// <summary>
/// Verifies that the range configuration field is wired up on each converted configuration editor, so that
/// saving a data type with a maximum lower than the minimum is rejected server-side.
/// </summary>
[TestFixture]
public class RangeConfigurationEditorValidationTests
{
    private static IIOHelper IOHelper => Mock.Of<IIOHelper>();

    private static object[] Editors =>
    [
        new object[] { new IntegerConfigurationEditor(IOHelper), "validationRange" },
        new object[] { new DecimalConfigurationEditor(IOHelper), "validationRange" },
        new object[] { new MediaPicker3ConfigurationEditor(IOHelper), "validationLimit" },
        new object[] { new MultiUrlPickerConfigurationEditor(IOHelper), "validationLimit" },
        new object[] { new MultiNodePickerConfigurationEditor(IOHelper), "validationLimit" },
    ];

    [TestCaseSource(nameof(Editors))]
    public void Cannot_Validate_When_Max_Is_Less_Than_Min(IConfigurationEditor editor, string rangeKey)
    {
        var configuration = new Dictionary<string, object>
        {
            [rangeKey] = new JsonObject { ["min"] = 5, ["max"] = 2 },
        };

        Assert.That(editor.Validate(configuration), Is.Not.Empty);
    }

    [TestCaseSource(nameof(Editors))]
    public void Can_Validate_When_Max_Is_Greater_Than_Min(IConfigurationEditor editor, string rangeKey)
    {
        var configuration = new Dictionary<string, object>
        {
            [rangeKey] = new JsonObject { ["min"] = 2, ["max"] = 5 },
        };

        Assert.That(editor.Validate(configuration), Is.Empty);
    }
}
