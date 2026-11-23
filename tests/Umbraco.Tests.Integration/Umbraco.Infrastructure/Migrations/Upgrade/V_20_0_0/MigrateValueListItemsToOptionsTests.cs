// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Text.Json.Nodes;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_20_0_0;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Migrations.Upgrade.V19_0_0;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Migrations.Upgrade.V20_0_0;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class MigrateValueListItemsToOptionsTests : MigrateMinMaxToRangeTestBase
{
    [TestCase(Constants.PropertyEditors.Aliases.SingleDropDown)]
    [TestCase(Constants.PropertyEditors.Aliases.MultipleDropDown)]
    [TestCase(Constants.PropertyEditors.Aliases.CheckBoxList)]
    [TestCase(Constants.PropertyEditors.Aliases.RadioButtonList)]
    public async Task Converts_String_Items_To_Option_Objects(string editorAlias)
    {
        var id = await CreateDataTypeWithRawConfig(editorAlias, """{"items":["a","b"]}""");

        await RunMigration<MigrateValueListItemsToOptions>();

        await AssertConfig(id, """{"items":[{"value":"a","label":"a"},{"value":"b","label":"b"}]}""");
    }

    [Test]
    public async Task Preserves_Other_Keys()
    {
        var id = await CreateDataTypeWithRawConfig(
            Constants.PropertyEditors.Aliases.CheckBoxList,
            """{"items":["a"],"foo":true}""");

        await RunMigration<MigrateValueListItemsToOptions>();

        await AssertConfig(id, """{"items":[{"value":"a","label":"a"}],"foo":true}""");
    }

    [TestCase("{}")]
    [TestCase("""{"items":[]}""")]
    [TestCase("null")]
    public async Task Leaves_Empty_Or_Missing_Items(string config)
    {
        var id = await CreateDataTypeWithRawConfig(Constants.PropertyEditors.Aliases.CheckBoxList, config);

        await RunMigration<MigrateValueListItemsToOptions>();

        await AssertConfig(id, config);
    }

    [Test]
    public async Task Matches_Keys_Regardless_Of_Casing()
    {
        var id = await CreateDataTypeWithRawConfig(Constants.PropertyEditors.Aliases.CheckBoxList, """{"Items":["a","b"]}""");

        await RunMigration<MigrateValueListItemsToOptions>();

        await AssertConfig(id, """{"Items":[{"value":"a","label":"a"},{"value":"b","label":"b"}]}""");
    }

    [Test]
    public async Task Ignores_Other_Editors()
    {
        const string Config = """{"items":["a","b"]}""";
        var id = await CreateDataTypeWithRawConfig(Constants.PropertyEditors.Aliases.MultipleTextstring, Config);

        await RunMigration<MigrateValueListItemsToOptions>();

        await AssertConfig(id, Config);
    }

    [Test]
    public async Task Is_Idempotent()
    {
        var id = await CreateDataTypeWithRawConfig(Constants.PropertyEditors.Aliases.CheckBoxList, """{"items":["a"]}""");

        await RunMigration<MigrateValueListItemsToOptions>();
        await RunMigration<MigrateValueListItemsToOptions>();

        await AssertConfig(id, """{"items":[{"value":"a","label":"a"}]}""");
    }

    private Task AssertConfig(int id, string expectedJson)
    {
        using IScope scope = ScopeProvider.CreateScope();
        var actualJson = scope.Database.ExecuteScalar<string>("SELECT config FROM umbracoDataType WHERE nodeId = @0", id);
        scope.Complete();

        Assert.IsTrue(
            JsonNode.DeepEquals(JsonNode.Parse(expectedJson), JsonNode.Parse(actualJson!)),
            $"Expected {expectedJson} but was {actualJson}");
        return Task.CompletedTask;
    }
}
