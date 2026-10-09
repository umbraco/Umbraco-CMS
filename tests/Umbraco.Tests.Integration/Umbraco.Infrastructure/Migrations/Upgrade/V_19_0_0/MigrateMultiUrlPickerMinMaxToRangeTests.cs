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
internal sealed class MigrateMultiUrlPickerMinMaxToRangeTests : MigrateMinMaxToRangeTestBase
{
    [Test]
    public async Task Combines_Min_And_Max_Into_ValidationLimit()
    {
        var id = await CreateDataTypeWithRawConfig(
            Constants.PropertyEditors.Aliases.MultiUrlPicker,
            """{ "minNumber": 2, "maxNumber": 5 }""");

        await RunMigration<MigrateMultiUrlPickerMinMaxToRange>();

        JsonObject config = await GetRawConfig(id);
        Assert.Multiple(() =>
        {
            Assert.That(config.ContainsKey("minNumber"), Is.False);
            Assert.That(config.ContainsKey("maxNumber"), Is.False);
            Assert.That(Min(config, "validationLimit"), Is.EqualTo(2));
            Assert.That(Max(config, "validationLimit"), Is.EqualTo(5));
        });
    }

    [Test]
    public async Task Zero_Values_Become_Unbounded()
    {
        var id = await CreateDataTypeWithRawConfig(
            Constants.PropertyEditors.Aliases.MultiUrlPicker,
            """{ "minNumber": 0, "maxNumber": 0 }""");

        await RunMigration<MigrateMultiUrlPickerMinMaxToRange>();

        JsonObject config = await GetRawConfig(id);
        Assert.Multiple(() =>
        {
            Assert.That(config.ContainsKey("minNumber"), Is.False);
            Assert.That(config.ContainsKey("maxNumber"), Is.False);
            Assert.That(config.ContainsKey("validationLimit"), Is.False);
        });
    }

    [Test]
    public async Task Misconfigured_Range_Is_Migrated_Without_Rejection()
    {
        // The migration rewrites the config directly, bypassing the data type service - so a legacy
        // configuration where the minimum exceeds the maximum is carried across faithfully rather than
        // being rejected by the new range validation.
        var id = await CreateDataTypeWithRawConfig(
            Constants.PropertyEditors.Aliases.MultiUrlPicker,
            """{ "minNumber": 5, "maxNumber": 2 }""");

        await RunMigration<MigrateMultiUrlPickerMinMaxToRange>();

        JsonObject config = await GetRawConfig(id);
        Assert.Multiple(() =>
        {
            Assert.That(Min(config, "validationLimit"), Is.EqualTo(5));
            Assert.That(Max(config, "validationLimit"), Is.EqualTo(2));
        });
    }

    [Test]
    public async Task Is_Idempotent()
    {
        var id = await CreateDataTypeWithRawConfig(
            Constants.PropertyEditors.Aliases.MultiUrlPicker,
            """{ "minNumber": 2, "maxNumber": 5 }""");

        await RunMigration<MigrateMultiUrlPickerMinMaxToRange>();
        await RunMigration<MigrateMultiUrlPickerMinMaxToRange>();

        JsonObject config = await GetRawConfig(id);
        Assert.Multiple(() =>
        {
            Assert.That(Min(config, "validationLimit"), Is.EqualTo(2));
            Assert.That(Max(config, "validationLimit"), Is.EqualTo(5));
        });
    }
}
