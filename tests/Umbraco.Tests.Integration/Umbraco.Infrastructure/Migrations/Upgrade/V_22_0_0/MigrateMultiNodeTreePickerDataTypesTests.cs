// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Migrations;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_22_0_0;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Migrations.Upgrade.V22_0_0;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class MigrateMultiNodeTreePickerDataTypesTests : UmbracoIntegrationTest
{
    private const string MultiNodeTreePickerAlias = "Umbraco.MultiNodeTreePicker";

    private const string DynamicRoot = """{ "originAlias": "Root", "querySteps": [ { "unique": "1230499d-beaa-4026-8380-f6ad57a6ddb3", "alias": "NearestDescendantOrSelf", "anyOfDocTypeKeys": [ "ecfbe74b-42dc-4b2b-b150-6458aea17cff" ] } ] }""";

    private IDataTypeService DataTypeService => GetRequiredService<IDataTypeService>();

    private PropertyEditorCollection PropertyEditors => GetRequiredService<PropertyEditorCollection>();

    private IConfigurationEditorJsonSerializer ConfigurationEditorJsonSerializer => GetRequiredService<IConfigurationEditorJsonSerializer>();

    private IContentService ContentService => GetRequiredService<IContentService>();

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private IDocumentCacheService DocumentCacheService => GetRequiredService<IDocumentCacheService>();

    private IPublishedContentCache PublishedContentCache => GetRequiredService<IPublishedContentCache>();

    private IMigrationPlanExecutor MigrationPlanExecutor => new MigrationPlanExecutor(
        ScopeProvider,
        ScopeAccessor,
        LoggerFactory,
        GetRequiredService<IMigrationBuilder>(),
        GetRequiredService<IUmbracoDatabaseFactory>(),
        GetRequiredService<IDatabaseCacheRebuilder>(),
        GetRequiredService<DistributedCache>(),
        Mock.Of<IKeyValueService>(),
        GetRequiredService<IServiceScopeFactory>(),
        AppCaches.NoCache,
        GetRequiredService<IPublishedContentTypeFactory>());

    // The cache refreshers must run, so published content picks up the data types' new editors after the migration.
    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        builder.AddNotificationHandler<ContentTreeChangeNotification, ContentTreeChangeDistributedCacheNotificationHandler>();
        builder.Services.AddUnique<IServerMessenger, ContentEventsTests.LocalServerMessenger>();
    }

    [TestCase(
        $$"""{ "startNode": { "type": "content", "id": "ca4249ed-2b23-4337-b522-63cabe5587d1", "dynamicRoot": {{DynamicRoot}} }, "filter": "7cb50cd2-4843-4bc8-bec0-1e458714d951", "validationLimit": { "max": 1 }, "ignoreUserStartNodes": true }""",
        "Umbraco.ContentPicker",
        "Umb.PropertyEditorUi.DocumentPicker",
        "Nvarchar",
        $$"""{ "ignoreUserStartNodes": true, "allowedContentTypes": "7cb50cd2-4843-4bc8-bec0-1e458714d951", "startNodeId": "ca4249ed-2b23-4337-b522-63cabe5587d1", "dynamicRoot": {{DynamicRoot}} }""",
        TestName = "Content mode holding one item moves to the document picker")]
    [TestCase(
        $$"""{ "startNode": { "type": "content", "dynamicRoot": {{DynamicRoot}} }, "filter": "7cb50cd2-4843-4bc8-bec0-1e458714d951", "validationLimit": { "min": 1, "max": 5 }, "ignoreUserStartNodes": false }""",
        "Umbraco.DocumentPicker.Multiple",
        "Umb.PropertyEditorUi.DocumentPicker.Multiple",
        "Ntext",
        $$"""{ "validationLimit": { "min": 1, "max": 5 }, "ignoreUserStartNodes": false, "allowedContentTypes": "7cb50cd2-4843-4bc8-bec0-1e458714d951", "dynamicRoot": {{DynamicRoot}} }""",
        TestName = "Content mode holding several items moves to the multiple document picker")]
    [TestCase(
        "{}",
        "Umbraco.DocumentPicker.Multiple",
        "Umb.PropertyEditorUi.DocumentPicker.Multiple",
        "Ntext",
        "{}",
        TestName = "No configuration is content mode without a maximum")]
    [TestCase(
        """{ "validationLimit": { "max": 1 } }""",
        "Umbraco.ContentPicker",
        "Umb.PropertyEditorUi.DocumentPicker",
        "Nvarchar",
        "{}",
        TestName = "No start node type holding one item moves to the document picker")]
    [TestCase(
        """{ "startNode": { "type": "content", "query": null, "dynamicRoot": null, "id": null }, "filter": "", "ignoreUserStartNodes": false }""",
        "Umbraco.DocumentPicker.Multiple",
        "Umb.PropertyEditorUi.DocumentPicker.Multiple",
        "Ntext",
        """{ "ignoreUserStartNodes": false }""",
        TestName = "Null and empty settings are not carried over")]
    [TestCase(
        """{ "startNode": { "type": "media", "id": "f0c1f0c1-e999-441d-a255-80cb70cea9eb" }, "filter": "cc07b313-0843-4aa8-bbda-871c8da728c8", "validationLimit": { "max": 1 }, "ignoreUserStartNodes": true }""",
        "Umbraco.MediaPicker.Single",
        "Umb.PropertyEditorUi.MediaPicker.Single",
        "Ntext",
        """{ "ignoreUserStartNodes": true, "filter": "cc07b313-0843-4aa8-bbda-871c8da728c8", "startNodeId": "f0c1f0c1-e999-441d-a255-80cb70cea9eb" }""",
        TestName = "Media mode holding one item moves to the single media picker")]
    [TestCase(
        $$"""{ "startNode": { "type": "media", "dynamicRoot": {{DynamicRoot}} }, "filter": "cc07b313-0843-4aa8-bbda-871c8da728c8", "validationLimit": { "min": 1, "max": 5 } }""",
        "Umbraco.MediaPicker3",
        "Umb.PropertyEditorUi.MediaPicker",
        "Ntext",
        """{ "validationLimit": { "min": 1, "max": 5 }, "filter": "cc07b313-0843-4aa8-bbda-871c8da728c8" }""",
        TestName = "Media mode holding several items moves to the media picker without its dynamic root")]
    [TestCase(
        """{ "startNode": { "type": "member" }, "filter": "d59be02f-1df9-4228-aa1e-01917d806cda", "validationLimit": { "max": 1 }, "ignoreUserStartNodes": true }""",
        "Umbraco.MemberPicker",
        "Umb.PropertyEditorUi.MemberPicker",
        "Nvarchar",
        """{ "filter": "d59be02f-1df9-4228-aa1e-01917d806cda" }""",
        TestName = "Member mode holding one item moves to the member picker")]
    [TestCase(
        """{ "startNode": { "type": "member", "query": "$root" }, "filter": "d59be02f-1df9-4228-aa1e-01917d806cda", "validationLimit": { "min": 1, "max": 5 } }""",
        "Umbraco.MemberPicker.Multiple",
        "Umb.PropertyEditorUi.MemberPicker.Multiple",
        "Ntext",
        """{ "validationLimit": { "min": 1, "max": 5 }, "filter": "d59be02f-1df9-4228-aa1e-01917d806cda" }""",
        TestName = "Member mode holding several items moves to the multiple member picker")]
    public async Task Moves_The_Data_Type_Onto_The_Dedicated_Picker(
        string configuration,
        string expectedEditorAlias,
        string expectedEditorUiAlias,
        string expectedDbType,
        string expectedConfiguration)
    {
        var id = await CreateMultiNodeTreePickerDataType(configuration);

        await RunMigration();

        DataTypeDto row = GetRow(id);
        Assert.Multiple(() =>
        {
            Assert.AreEqual(expectedEditorAlias, row.EditorAlias);
            Assert.AreEqual(expectedEditorUiAlias, row.EditorUiAlias);
            Assert.AreEqual(expectedDbType, row.DbType);
            Assert.That(
                JsonNode.DeepEquals(JsonNode.Parse(row.Configuration!), JsonNode.Parse(expectedConfiguration)),
                Is.True,
                $"Configuration was {row.Configuration}");
        });
    }

    [TestCase("""{ "startNode": { "type": "somethingElse" }, "filter": "abc" }""", TestName = "Leaves a data type with an unknown start node type")]
    [TestCase("not json", TestName = "Leaves a data type with configuration that isn't JSON")]
    [TestCase("[]", TestName = "Leaves a data type with configuration that is a JSON array")]
    [TestCase("\"text\"", TestName = "Leaves a data type with configuration that is a JSON string")]
    public async Task Leaves_A_Data_Type_It_Cannot_Map(string configuration)
    {
        var id = await CreateMultiNodeTreePickerDataType(configuration);

        await RunMigration();

        DataTypeDto row = GetRow(id);
        Assert.Multiple(() =>
        {
            Assert.AreEqual(MultiNodeTreePickerAlias, row.EditorAlias);
            Assert.AreEqual(configuration, row.Configuration);
        });
    }

    [Test]
    public async Task Leaves_Data_Types_Using_Other_Editors()
    {
        var dataType = new DataType(PropertyEditors[Constants.PropertyEditors.Aliases.ContentPicker], ConfigurationEditorJsonSerializer)
        {
            Name = "Content picker",
            DatabaseType = ValueStorageType.Nvarchar,
        };
        await DataTypeService.CreateAsync(dataType, Constants.Security.SuperUserKey);

        await RunMigration();

        Assert.AreEqual(Constants.PropertyEditors.Aliases.ContentPicker, GetRow(dataType.Id).EditorAlias);
    }

    [Test]
    public async Task Values_Stored_By_The_Multi_Node_Tree_Picker_Render_After_The_Migration()
    {
        IContentType targetType = new ContentTypeBuilder().WithAlias("target").WithName("Target").Build();
        await ContentTypeService.CreateAsync(targetType, Constants.Security.SuperUserKey);
        IContent first = CreateAndPublish(targetType, "First target");
        IContent second = CreateAndPublish(targetType, "Second target");

        var dataTypeId = await CreateMultiNodeTreePickerDataType("""{ "startNode": { "type": "content" } }""");
        IContentType pageType = new ContentTypeBuilder()
            .WithAlias("pickerPage")
            .WithName("Picker Page")
            .AddPropertyType()
            .WithAlias("picker")
            .WithName("Picker")
            .WithDataTypeId(dataTypeId)
            .Done()
            .Build();
        await ContentTypeService.CreateAsync(pageType, Constants.Security.SuperUserKey);
        IContent page = CreateAndPublish(
            pageType,
            "Picker Page",
            new { picker = $"{new GuidUdi(Constants.UdiEntityType.Document, first.Key)},{new GuidUdi(Constants.UdiEntityType.Document, second.Key)}" });

        await RunMigration();

        IPublishedContent? published = PublishedContentCache.GetById(page.Key);
        Assert.Multiple(() =>
        {
            Assert.AreEqual(
                Constants.PropertyEditors.Aliases.MultipleDocumentPicker,
                published?.ContentType.GetPropertyType("picker")?.EditorAlias);
            Assert.That(
                published?.Value<IEnumerable<IPublishedContent>>("picker")?.Select(x => x.Key),
                Is.EqualTo(new[] { first.Key, second.Key }));
        });
    }

    private async Task<int> CreateMultiNodeTreePickerDataType(string configuration)
    {
        var dataType = new DataType(PropertyEditors[MultiNodeTreePickerAlias], ConfigurationEditorJsonSerializer)
        {
            Name = $"Picker {Guid.NewGuid()}",
            DatabaseType = ValueStorageType.Ntext,
        };
        await DataTypeService.CreateAsync(dataType, Constants.Security.SuperUserKey);

        // Overwrite with the exact configuration, as stored by the multi node tree picker.
        using IScope scope = ScopeProvider.CreateScope();
        scope.Database.Execute("UPDATE umbracoDataType SET config = @0 WHERE nodeId = @1", configuration, dataType.Id);
        scope.Complete();

        return dataType.Id;
    }

    private DataTypeDto GetRow(int nodeId)
    {
        using IScope scope = ScopeProvider.CreateScope();
        DataTypeDto row = scope.Database.Single<DataTypeDto>(
            scope.SqlContext.Sql().Select<DataTypeDto>().From<DataTypeDto>().Where<DataTypeDto>(x => x.NodeId == nodeId));
        scope.Complete();
        return row;
    }

    private IContent CreateAndPublish(IContentType contentType, string name, object? propertyValues = null)
    {
        ContentBuilder builder = new ContentBuilder().WithContentType(contentType).WithName(name);
        if (propertyValues is not null)
        {
            builder.WithPropertyValues(propertyValues);
        }

        IContent content = builder.Build();
        ContentService.Save(content);
        Assert.IsTrue(ContentService.Publish(content, ["*"]).Success);
        DocumentCacheService.RefreshContentAsync(content).GetAwaiter().GetResult();
        return content;
    }

    private async Task RunMigration()
    {
        MigrationPlan plan = new MigrationPlan(nameof(MigrateMultiNodeTreePickerDataTypes))
            .From(string.Empty)
            .To<MigrateMultiNodeTreePickerDataTypes>("done");
        await new Upgrader(plan).ExecuteAsync(MigrationPlanExecutor, ScopeProvider, Mock.Of<IKeyValueService>());
    }
}
