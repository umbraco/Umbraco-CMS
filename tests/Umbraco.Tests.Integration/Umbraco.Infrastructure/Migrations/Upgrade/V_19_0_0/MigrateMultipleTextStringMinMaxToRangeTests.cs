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
internal sealed class MigrateMultipleTextStringMinMaxToRangeTests : MigrateMinMaxToRangeTestBase
{
    [Test]
    public async Task Combines_Min_And_Max_Into_ValidationLimit()
    {
        var id = await CreateDataTypeWithRawConfig(
            Constants.PropertyEditors.Aliases.MultipleTextstring,
            """{ "min": 1, "max": 3 }""");

        await RunMigration<MigrateMultipleTextStringMinMaxToRange>();

        JsonObject config = await GetRawConfig(id);
        Assert.Multiple(() =>
        {
            Assert.That(config.ContainsKey("min"), Is.False);
            Assert.That(config.ContainsKey("max"), Is.False);
            Assert.That(Min(config, "validationLimit"), Is.EqualTo(1));
            Assert.That(Max(config, "validationLimit"), Is.EqualTo(3));
        });
    }
}
