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
internal sealed class MigrateIntegerMinMaxToRangeTests : MigrateMinMaxToRangeTestBase
{
    [Test]
    public async Task Combines_Into_ValidationRange_And_Preserves_Step()
    {
        var id = await CreateDataTypeWithRawConfig(
            Constants.PropertyEditors.Aliases.Integer,
            """{ "min": 1, "max": 10, "step": 2 }""");

        await RunMigration<MigrateIntegerMinMaxToRange>();

        JsonObject config = await GetRawConfig(id);
        Assert.Multiple(() =>
        {
            Assert.That(config.ContainsKey("min"), Is.False);
            Assert.That(config.ContainsKey("max"), Is.False);
            Assert.That(Min(config, "validationRange"), Is.EqualTo(1));
            Assert.That(Max(config, "validationRange"), Is.EqualTo(10));
            Assert.That(config["step"]?.GetValue<int>(), Is.EqualTo(2));
        });
    }
}
