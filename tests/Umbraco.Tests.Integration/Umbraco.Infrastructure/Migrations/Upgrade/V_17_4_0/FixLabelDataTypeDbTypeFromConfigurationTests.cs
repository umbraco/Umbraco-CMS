// Copyright (c) Umbraco.
// See LICENSE for more details.

using NPoco;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_17_4_0;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Migrations.Upgrade.V17_4_0;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class FixLabelDataTypeDbTypeFromConfigurationTests : UmbracoIntegrationTest
{
    private IDataTypeService DataTypeService => GetRequiredService<IDataTypeService>();

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private IContentEditingService ContentEditingService => GetRequiredService<IContentEditingService>();

    private PropertyEditorCollection PropertyEditors => GetRequiredService<PropertyEditorCollection>();

    private IConfigurationEditorJsonSerializer ConfigurationEditorJsonSerializer
        => GetRequiredService<IConfigurationEditorJsonSerializer>();

    [Test]
    public async Task Corrects_DbType_For_Label_Configured_As_LongString()
    {
        IDataType dataType = await CreateLabelDataType(ValueTypes.Text);
        await SetDbTypeInDatabase(dataType.Id, ValueStorageType.Nvarchar);
        await AssertDbTypeInDatabase(dataType.Id, ValueStorageType.Nvarchar);

        await ExecuteMigration();

        await AssertDbTypeInDatabase(dataType.Id, ValueStorageType.Ntext);
    }

    [Test]
    public async Task Leaves_DbType_Unchanged_For_Label_Configured_As_String()
    {
        IDataType dataType = await CreateLabelDataType(ValueTypes.String);
        await AssertDbTypeInDatabase(dataType.Id, ValueStorageType.Nvarchar);

        await ExecuteMigration();

        await AssertDbTypeInDatabase(dataType.Id, ValueStorageType.Nvarchar);
    }

    [Test]
    public async Task Treats_Missing_ValueType_Config_As_String()
    {
        IDataType dataType = await CreateLabelDataType(valueType: null);
        await AssertDbTypeInDatabase(dataType.Id, ValueStorageType.Nvarchar);

        await ExecuteMigration();

        await AssertDbTypeInDatabase(dataType.Id, ValueStorageType.Nvarchar);
    }

    [Test]
    public async Task Is_Idempotent_When_DbType_Already_Correct()
    {
        IDataType dataType = await CreateLabelDataType(ValueTypes.Text);
        await AssertDbTypeInDatabase(dataType.Id, ValueStorageType.Ntext);

        await ExecuteMigration();
        await ExecuteMigration();

        await AssertDbTypeInDatabase(dataType.Id, ValueStorageType.Ntext);
    }

    [Test]
    public async Task Relocates_VarcharValue_Content_To_TextValue_When_Moving_To_Ntext()
    {
        IDataType dataType = await CreateLabelDataType(ValueTypes.Text);
        IContentType contentType = await CreateContentTypeWithLabelProperty(dataType);
        IContent content = await CreateContent(contentType);

        // Simulate the pre-migration state: dbType is Nvarchar, and the property value lives in varcharValue.
        // (The Label editor is read-only so ContentEditingService does not persist a value for us; insert one directly.)
        await SetDbTypeInDatabase(dataType.Id, ValueStorageType.Nvarchar);
        var propertyTypeId = contentType.PropertyTypes.Single(pt => pt.Alias == "testLabel").Id;
        await InsertPropertyDataVarcharValue(content.Id, propertyTypeId, "stored-as-varchar");

        await ExecuteMigration();

        await AssertDbTypeInDatabase(dataType.Id, ValueStorageType.Ntext);
        await AssertPropertyDataMoved(propertyTypeId, expectedTextValue: "stored-as-varchar");
    }

    [Test]
    public async Task Relocates_VarcharValue_Content_When_Label_Is_Already_Ntext()
    {
        // Covers the workaround scenario: a user manually re-saved the Label data type after upgrading,
        // so dbType is already Ntext, but any content saved before that re-save is still stuck in
        // varcharValue. The migration's relocation step must run regardless of whether this run
        // transitioned the data type, and must move those legacy rows into textValue.
        IDataType dataType = await CreateLabelDataType(ValueTypes.Text);
        IContentType contentType = await CreateContentTypeWithLabelProperty(dataType);
        IContent content = await CreateContent(contentType);

        await AssertDbTypeInDatabase(dataType.Id, ValueStorageType.Ntext);
        var propertyTypeId = contentType.PropertyTypes.Single(pt => pt.Alias == "testLabel").Id;
        await InsertPropertyDataVarcharValue(content.Id, propertyTypeId, "legacy-varchar");

        await ExecuteMigration();

        await AssertDbTypeInDatabase(dataType.Id, ValueStorageType.Ntext);
        await AssertPropertyDataMoved(propertyTypeId, expectedTextValue: "legacy-varchar");
    }

    [Test]
    public async Task Leaves_Existing_TextValue_Untouched_When_Moving_To_Ntext()
    {
        // Scenario A: an upgraded v13 database already has the value in textValue (because v13 stored
        // Long-String Label values there). Only the dbType is wrong; the relocation UPDATE must leave
        // the textValue row alone — its AND varcharValue IS NOT NULL guard is what protects it.
        IDataType dataType = await CreateLabelDataType(ValueTypes.Text);
        IContentType contentType = await CreateContentTypeWithLabelProperty(dataType);
        IContent content = await CreateContent(contentType);

        await SetDbTypeInDatabase(dataType.Id, ValueStorageType.Nvarchar);
        var propertyTypeId = contentType.PropertyTypes.Single(pt => pt.Alias == "testLabel").Id;
        await InsertPropertyDataTextValue(content.Id, propertyTypeId, "stored-as-textvalue");

        await ExecuteMigration();

        await AssertDbTypeInDatabase(dataType.Id, ValueStorageType.Ntext);
        await AssertPropertyDataMoved(propertyTypeId, expectedTextValue: "stored-as-textvalue");
    }

    private async Task<IDataType> CreateLabelDataType(string? valueType)
    {
        var editor = PropertyEditors.First(e => e.Alias == Constants.PropertyEditors.Aliases.Label);
        var dataType = new DataType(editor, ConfigurationEditorJsonSerializer)
        {
            Name = $"Test Label {valueType ?? "Default"}",
            DatabaseType = valueType is null ? ValueStorageType.Nvarchar : ValueTypes.ToStorageType(valueType),
        };

        if (valueType is not null)
        {
            dataType.ConfigurationData = new Dictionary<string, object>
            {
                [Constants.PropertyEditors.ConfigurationKeys.DataValueType] = valueType,
            };
        }

        await DataTypeService.CreateAsync(dataType, Constants.Security.SuperUserKey);
        return (await DataTypeService.GetAsync(dataType.Key))!;
    }

    private async Task<IContentType> CreateContentTypeWithLabelProperty(IDataType dataType)
    {
        var contentType = new ContentTypeBuilder()
            .WithAlias("migrationTestContentType")
            .WithName("Migration Test Content Type")
            .WithIsElement(false)
            .AddPropertyType()
                .WithAlias("testLabel")
                .WithName("Test Label")
                .WithDataTypeId(dataType.Id)
                .Done()
            .Build();
        contentType.AllowedAsRoot = true;

        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);
        return (await ContentTypeService.GetAsync(contentType.Key))!;
    }

    private async Task<IContent> CreateContent(IContentType contentType)
    {
        var createModel = new ContentCreateModel
        {
            Key = Guid.NewGuid(),
            ContentTypeKey = contentType.Key,
            ParentKey = Constants.System.RootKey,
            Variants = [new VariantModel { Name = "Migration Test Content" }],
            Properties =
            [
                new PropertyValueModel
                {
                    Alias = "testLabel",
                    Value = "initial-value",
                },
            ],
        };

        var result = await ContentEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success, $"Failed to create content: {result.Status}");
        return result.Result.Content!;
    }

    private async Task SetDbTypeInDatabase(int dataTypeId, ValueStorageType dbType)
    {
        using IScope scope = ScopeProvider.CreateScope();
        await scope.Database.ExecuteAsync(
            "UPDATE umbracoDataType SET dbType = @0 WHERE nodeId = @1",
            dbType.ToString(),
            dataTypeId);
        scope.Complete();
    }

    private async Task InsertPropertyDataVarcharValue(int contentId, int propertyTypeId, string value)
    {
        using IScope scope = ScopeProvider.CreateScope();

        // Find the current version for the content so the property data row is associated with real state.
        var versionId = await scope.Database.ExecuteScalarAsync<int>(
            "SELECT id FROM umbracoContentVersion WHERE nodeId = @0 AND [current] = 1",
            contentId);

        var propertyDataDto = new PropertyDataDto
        {
            VersionId = versionId,
            PropertyTypeId = propertyTypeId,
            LanguageId = null,
            Segment = null,
            VarcharValue = value,
        };
        await scope.Database.InsertAsync(propertyDataDto);

        scope.Complete();
    }

    private async Task InsertPropertyDataTextValue(int contentId, int propertyTypeId, string value)
    {
        using IScope scope = ScopeProvider.CreateScope();

        var versionId = await scope.Database.ExecuteScalarAsync<int>(
            "SELECT id FROM umbracoContentVersion WHERE nodeId = @0 AND [current] = 1",
            contentId);

        var propertyDataDto = new PropertyDataDto
        {
            VersionId = versionId,
            PropertyTypeId = propertyTypeId,
            LanguageId = null,
            Segment = null,
            TextValue = value,
        };
        await scope.Database.InsertAsync(propertyDataDto);

        scope.Complete();
    }

    private async Task AssertDbTypeInDatabase(int dataTypeId, ValueStorageType expected)
    {
        using IScope scope = ScopeProvider.CreateScope();
        var actual = await scope.Database.ExecuteScalarAsync<string>(
            "SELECT dbType FROM umbracoDataType WHERE nodeId = @0",
            dataTypeId);
        scope.Complete();
        Assert.AreEqual(expected.ToString(), actual);
    }

    private async Task AssertPropertyDataMoved(int propertyTypeId, string expectedTextValue)
    {
        using IScope scope = ScopeProvider.CreateScope();
        Sql<ISqlContext> sql = scope.Database.SqlContext.Sql()
            .Select<PropertyDataDto>()
            .From<PropertyDataDto>()
            .Where<PropertyDataDto>(x => x.PropertyTypeId == propertyTypeId);
        var rows = await scope.Database.FetchAsync<PropertyDataDto>(sql);
        scope.Complete();

        Assert.IsNotEmpty(rows);
        foreach (var row in rows)
        {
            Assert.AreEqual(expectedTextValue, row.TextValue);
            Assert.IsNull(row.VarcharValue);
        }
    }

    private async Task ExecuteMigration()
    {
        using IScope scope = ScopeProvider.CreateScope();
        await FixLabelDataTypeDbTypeFromConfiguration.ExecuteMigration(scope.Database, DataTypeService);
        scope.Complete();
    }
}
