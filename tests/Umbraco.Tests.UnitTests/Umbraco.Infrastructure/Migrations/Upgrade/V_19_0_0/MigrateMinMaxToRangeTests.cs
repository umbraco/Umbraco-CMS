using System.Text.Json.Nodes;
using NUnit.Framework;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_19_0_0;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Migrations.Upgrade.V_19_0_0;

[TestFixture]
public class MigrateMinMaxToRangeTests
{
    private static decimal? Min(JsonObject configuration, string rangeKey)
        => (configuration[rangeKey] as JsonObject)?["min"]?.GetValue<decimal>();

    private static decimal? Max(JsonObject configuration, string rangeKey)
        => (configuration[rangeKey] as JsonObject)?["max"]?.GetValue<decimal>();

    [Test]
    public void Count_Editor_Combines_Min_And_Max_Into_Range()
    {
        var configuration = new JsonObject { ["minNumber"] = 2, ["maxNumber"] = 5 };

        var changed = MigrateMinMaxToRangeMigrationBase.MigrateTopLevelRange(
            configuration, "minNumber", "maxNumber", "validationLimit", minZeroIsUnbounded: true, maxZeroIsUnbounded: true);

        Assert.Multiple(() =>
        {
            Assert.That(changed, Is.True);
            Assert.That(configuration.ContainsKey("minNumber"), Is.False);
            Assert.That(configuration.ContainsKey("maxNumber"), Is.False);
            Assert.That(Min(configuration, "validationLimit"), Is.EqualTo(2));
            Assert.That(Max(configuration, "validationLimit"), Is.EqualTo(5));
        });
    }

    [Test]
    public void Count_Editor_Treats_Zero_As_Unbounded_And_Omits_Empty_Range()
    {
        var configuration = new JsonObject { ["minNumber"] = 0, ["maxNumber"] = 0 };

        var changed = MigrateMinMaxToRangeMigrationBase.MigrateTopLevelRange(
            configuration, "minNumber", "maxNumber", "validationLimit", minZeroIsUnbounded: true, maxZeroIsUnbounded: true);

        Assert.Multiple(() =>
        {
            Assert.That(changed, Is.True);
            Assert.That(configuration.ContainsKey("minNumber"), Is.False);
            Assert.That(configuration.ContainsKey("maxNumber"), Is.False);
            Assert.That(configuration.ContainsKey("validationLimit"), Is.False);
        });
    }

    [Test]
    public void Migration_Is_A_No_Op_When_Keys_Are_Absent()
    {
        var configuration = new JsonObject { ["filter"] = "abc" };

        var changed = MigrateMinMaxToRangeMigrationBase.MigrateTopLevelRange(
            configuration, "minNumber", "maxNumber", "validationLimit", minZeroIsUnbounded: true, maxZeroIsUnbounded: true);

        Assert.Multiple(() =>
        {
            Assert.That(changed, Is.False);
            Assert.That(configuration.ContainsKey("validationLimit"), Is.False);
        });
    }

    [Test]
    public void Misconfigured_Min_Greater_Than_Max_Is_Carried_Across_Faithfully()
    {
        var configuration = new JsonObject { ["minNumber"] = 5, ["maxNumber"] = 2 };

        MigrateMinMaxToRangeMigrationBase.MigrateTopLevelRange(
            configuration, "minNumber", "maxNumber", "validationLimit", minZeroIsUnbounded: true, maxZeroIsUnbounded: true);

        Assert.Multiple(() =>
        {
            Assert.That(Min(configuration, "validationLimit"), Is.EqualTo(5));
            Assert.That(Max(configuration, "validationLimit"), Is.EqualTo(2));
        });
    }

    [Test]
    public void Slider_Preserves_Zero_Minimum_But_Treats_Zero_Maximum_As_Unbounded()
    {
        var configuration = new JsonObject { ["minVal"] = 0, ["maxVal"] = 0 };

        MigrateMinMaxToRangeMigrationBase.MigrateTopLevelRange(
            configuration, "minVal", "maxVal", "validationRange", minZeroIsUnbounded: false, maxZeroIsUnbounded: true);

        Assert.Multiple(() =>
        {
            Assert.That(Min(configuration, "validationRange"), Is.EqualTo(0));
            Assert.That(Max(configuration, "validationRange"), Is.Null);
        });
    }

    [Test]
    public void Slider_Preserves_Decimal_Bounds()
    {
        var configuration = new JsonObject { ["minVal"] = 1.5, ["maxVal"] = 9.5 };

        MigrateMinMaxToRangeMigrationBase.MigrateTopLevelRange(
            configuration, "minVal", "maxVal", "validationRange", minZeroIsUnbounded: false, maxZeroIsUnbounded: true);

        Assert.Multiple(() =>
        {
            Assert.That(Min(configuration, "validationRange"), Is.EqualTo(1.5m));
            Assert.That(Max(configuration, "validationRange"), Is.EqualTo(9.5m));
        });
    }

    [Test]
    public void Block_Grid_Areas_Are_Migrated_Per_Area()
    {
        var configuration = new JsonObject
        {
            ["blocks"] = new JsonArray(
                new JsonObject
                {
                    ["areas"] = new JsonArray(
                        new JsonObject { ["minAllowed"] = 1, ["maxAllowed"] = 3 },
                        new JsonObject { ["alias"] = "no-limits" }),
                }),
        };

        var changed = MigrateBlockGridAreaMinMaxToRange.MigrateAreas(configuration);

        var areas = (JsonArray)((JsonObject)configuration["blocks"]![0]!)["areas"]!;
        var firstArea = (JsonObject)areas[0]!;
        var secondArea = (JsonObject)areas[1]!;

        Assert.Multiple(() =>
        {
            Assert.That(changed, Is.True);
            Assert.That(firstArea.ContainsKey("minAllowed"), Is.False);
            Assert.That(firstArea.ContainsKey("maxAllowed"), Is.False);
            Assert.That(Min(firstArea, "validationLimit"), Is.EqualTo(1));
            Assert.That(Max(firstArea, "validationLimit"), Is.EqualTo(3));
            Assert.That(secondArea.ContainsKey("validationLimit"), Is.False);
        });
    }

    [Test]
    public void Block_Grid_Migration_Is_A_No_Op_Without_Blocks()
    {
        var configuration = new JsonObject { ["gridColumns"] = 12 };

        Assert.That(MigrateBlockGridAreaMinMaxToRange.MigrateAreas(configuration), Is.False);
    }
}
