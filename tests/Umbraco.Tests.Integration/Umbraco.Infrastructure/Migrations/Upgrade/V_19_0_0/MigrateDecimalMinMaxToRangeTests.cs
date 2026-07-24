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
internal sealed class MigrateDecimalMinMaxToRangeTests : MigrateMinMaxToRangeTestBase
{
    [Test]
    public async Task Combines_Into_ValidationRange_And_Preserves_Step()
    {
        var id = await CreateDataTypeWithRawConfig(
            Constants.PropertyEditors.Aliases.Decimal,
            """{ "min": 0.5, "max": 9.5, "step": 0.1 }""");

        await RunMigration<MigrateDecimalMinMaxToRange>();

        JsonObject config = await GetRawConfig(id);
        Assert.Multiple(() =>
        {
            Assert.That(Min(config, "validationRange"), Is.EqualTo(0.5m));
            Assert.That(Max(config, "validationRange"), Is.EqualTo(9.5m));
            Assert.That(config["step"]?.GetValue<decimal>(), Is.EqualTo(0.1m));
        });
    }
}
