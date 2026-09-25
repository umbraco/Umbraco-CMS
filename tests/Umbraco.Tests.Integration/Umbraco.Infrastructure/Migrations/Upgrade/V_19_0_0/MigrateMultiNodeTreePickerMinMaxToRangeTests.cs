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
internal sealed class MigrateMultiNodeTreePickerMinMaxToRangeTests : MigrateMinMaxToRangeTestBase
{
    [Test]
    public async Task Combines_And_Preserves_Other_Config()
    {
        var id = await CreateDataTypeWithRawConfig(
            Constants.PropertyEditors.Aliases.MultiNodeTreePicker,
            """{ "minNumber": 1, "maxNumber": 4, "filter": "abc" }""");

        await RunMigration<MigrateMultiNodeTreePickerMinMaxToRange>();

        JsonObject config = await GetRawConfig(id);
        Assert.Multiple(() =>
        {
            Assert.That(config.ContainsKey("minNumber"), Is.False);
            Assert.That(config.ContainsKey("maxNumber"), Is.False);
            Assert.That(Min(config, "validationLimit"), Is.EqualTo(1));
            Assert.That(Max(config, "validationLimit"), Is.EqualTo(4));
            Assert.That(config["filter"]?.GetValue<string>(), Is.EqualTo("abc"));
        });
    }
}
