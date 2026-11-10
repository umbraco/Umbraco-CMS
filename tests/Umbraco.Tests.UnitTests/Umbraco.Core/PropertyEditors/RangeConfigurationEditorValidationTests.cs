using System.Text.Json;
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
        new object[] { new SliderConfigurationEditor(IOHelper), "validationRange" },
        new object[] { new RangeSliderConfigurationEditor(IOHelper), "validationRange" },
        new object[] { new MediaPicker3ConfigurationEditor(IOHelper), "validationLimit" },
        new object[] { new MultiUrlPickerConfigurationEditor(IOHelper), "validationLimit" },
        new object[] { new MultiNodePickerConfigurationEditor(IOHelper), "validationLimit" },
        new object[] { new MultipleDocumentPickerConfigurationEditor(IOHelper), "validationLimit" },
        new object[] { new MultipleMemberPickerConfigurationEditor(IOHelper), "validationLimit" },
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

    [Test]
    public void Cannot_Configure_Block_Grid_Area_With_Max_Less_Than_Min()
    {
        var configuration = new Dictionary<string, object>
        {
            ["blocks"] = CreateBlocksWithAreaLimit(5, 2),
        };

        Assert.That(new BlockGridConfigurationEditor(IOHelper).Validate(configuration), Is.Not.Empty);
    }

    [Test]
    public void Can_Configure_Block_Grid_Area_With_Max_Greater_Than_Min()
    {
        var configuration = new Dictionary<string, object>
        {
            ["blocks"] = CreateBlocksWithAreaLimit(2, 5),
        };

        Assert.That(new BlockGridConfigurationEditor(IOHelper).Validate(configuration), Is.Empty);
    }

    [Test]
    public void Cannot_Configure_Block_Grid_Area_With_Max_Less_Than_Min_From_A_Json_Element()
    {
        // Configuration posted to the Management API arrives as JSON elements.
        var configuration = new Dictionary<string, object>
        {
            ["blocks"] = JsonSerializer.SerializeToElement(CreateBlocksWithAreaLimit(5, 2)),
        };

        Assert.That(new BlockGridConfigurationEditor(IOHelper).Validate(configuration), Is.Not.Empty);
    }

    private static JsonArray CreateBlocksWithAreaLimit(int min, int max)
        => new(
            new JsonObject
            {
                ["areas"] = new JsonArray(
                    new JsonObject
                    {
                        ["alias"] = "main",
                        ["validationLimit"] = new JsonObject { ["min"] = min, ["max"] = max },
                    }),
            });
}
