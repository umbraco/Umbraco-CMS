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
internal sealed class MigrateBlockGridAreaMinMaxToRangeTests : MigrateMinMaxToRangeTestBase
{
    [Test]
    public async Task Combines_Area_Min_Max_Into_ValidationLimit_Per_Area()
    {
        var id = await CreateDataTypeWithRawConfig(
            Constants.PropertyEditors.Aliases.BlockGrid,
            """{ "blocks": [ { "areas": [ { "alias": "main", "minAllowed": 1, "maxAllowed": 3 } ] } ] }""");

        await RunMigration<MigrateBlockGridAreaMinMaxToRange>();

        JsonObject config = await GetRawConfig(id);
        var area = (JsonObject)((JsonArray)((JsonObject)((JsonArray)config["blocks"]!)[0]!)["areas"]!)[0]!;
        Assert.Multiple(() =>
        {
            Assert.That(area.ContainsKey("minAllowed"), Is.False);
            Assert.That(area.ContainsKey("maxAllowed"), Is.False);
            Assert.That(Min(area, "validationLimit"), Is.EqualTo(1));
            Assert.That(Max(area, "validationLimit"), Is.EqualTo(3));
        });
    }
}
