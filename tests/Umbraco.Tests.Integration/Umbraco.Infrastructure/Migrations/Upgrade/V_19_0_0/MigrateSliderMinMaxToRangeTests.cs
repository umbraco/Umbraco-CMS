// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Text.Json.Nodes;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_19_0_0;
using Umbraco.Cms.Tests.Common.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Migrations.Upgrade.V19_0_0;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class MigrateSliderMinMaxToRangeTests : MigrateMinMaxToRangeTestBase
{
    [Test]
    public async Task Combines_Into_ValidationRange_And_Preserves_Other_Config()
    {
        var id = await CreateDataTypeWithRawConfig(
            Constants.PropertyEditors.Aliases.Slider,
            """{ "minVal": 0, "maxVal": 100, "step": 1, "enableRange": true, "minimumRange": 0 }""");

        await RunMigration<MigrateSliderMinMaxToRange>();

        JsonObject config = await GetRawConfig(id);
        Assert.Multiple(() =>
        {
            Assert.That(config.ContainsKey("minVal"), Is.False);
            Assert.That(config.ContainsKey("maxVal"), Is.False);

            // Slider minimum of zero is a real lower bound and must be preserved.
            Assert.That(Min(config, "validationRange"), Is.EqualTo(0));
            Assert.That(Max(config, "validationRange"), Is.EqualTo(100));
            Assert.That(config["enableRange"]?.GetValue<bool>(), Is.True);
        });
    }

    [Test]
    public async Task Zero_Maximum_Becomes_Unbounded()
    {
        var id = await CreateDataTypeWithRawConfig(
            Constants.PropertyEditors.Aliases.Slider,
            """{ "minVal": 5, "maxVal": 0 }""");

        await RunMigration<MigrateSliderMinMaxToRange>();

        JsonObject config = await GetRawConfig(id);
        Assert.Multiple(() =>
        {
            Assert.That(Min(config, "validationRange"), Is.EqualTo(5));
            Assert.That(Max(config, "validationRange"), Is.Null);
        });
    }
}
