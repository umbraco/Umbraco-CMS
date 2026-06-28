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
using Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_17_5_0;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Migrations.Upgrade.V17_5_0;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class ConvertRichTextBlockUdisToKeysTests : UmbracoIntegrationTest
{
    // Mirrors the real stored format: the markup's attribute quotes are serialized as the JSON unicode
    // escape (backslash + u0022), written here with a doubled backslash so the literal sequence is stored.
    private const string LegacyValue =
        "{\"markup\":\"<p><umb-rte-block-inline data-content-udi=\\u0022umb://element/ccda6d01e3cd44dd883fdf9f5c84d3a7\\u0022></umb-rte-block-inline></p>\",\"blocks\":{}}";

    private const string MigratedValue =
        "{\"markup\":\"<p><umb-rte-block-inline data-content-key=\\u0022ccda6d01-e3cd-44dd-883f-df9f5c84d3a7\\u0022></umb-rte-block-inline></p>\",\"blocks\":{}}";

    private IDataTypeService DataTypeService => GetRequiredService<IDataTypeService>();

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private IContentEditingService ContentEditingService => GetRequiredService<IContentEditingService>();

    private PropertyEditorCollection PropertyEditors => GetRequiredService<PropertyEditorCollection>();

    private IConfigurationEditorJsonSerializer ConfigurationEditorJsonSerializer
        => GetRequiredService<IConfigurationEditorJsonSerializer>();

    [Test]
    public async Task Converts_Legacy_Block_Reference_In_TextValue()
    {
        var propertyTypeId = await CreatePropertyDataWithValue(LegacyValue);

        await ExecuteMigration();

        await AssertTextValue(propertyTypeId, MigratedValue);
    }

    [Test]
    public async Task Leaves_Already_Migrated_Value_Untouched()
    {
        var propertyTypeId = await CreatePropertyDataWithValue(MigratedValue);

        await ExecuteMigration();

        await AssertTextValue(propertyTypeId, MigratedValue);
    }

    private async Task<int> CreatePropertyDataWithValue(string textValue)
    {
        IDataType dataType = await CreateRichTextDataType();
        IContentType contentType = await CreateContentTypeWithProperty(dataType);
        IContent content = await CreateContent(contentType);

        var propertyTypeId = contentType.PropertyTypes.Single(pt => pt.Alias == "bodyText").Id;
        await InsertPropertyDataTextValue(content.Id, propertyTypeId, textValue);

        return propertyTypeId;
    }

    private async Task<IDataType> CreateRichTextDataType()
    {
        IDataEditor editor = PropertyEditors.First(e => e.Alias == Constants.PropertyEditors.Aliases.RichText);
        var dataType = new DataType(editor, ConfigurationEditorJsonSerializer)
        {
            Name = "Test Rich Text",
            DatabaseType = ValueStorageType.Ntext,
        };

        await DataTypeService.CreateAsync(dataType, Constants.Security.SuperUserKey);
        return (await DataTypeService.GetAsync(dataType.Key))!;
    }

    private async Task<IContentType> CreateContentTypeWithProperty(IDataType dataType)
    {
        var contentType = new ContentTypeBuilder()
            .WithAlias("migrationTestContentType")
            .WithName("Migration Test Content Type")
            .WithIsElement(false)
            .AddPropertyType()
                .WithAlias("bodyText")
                .WithName("Body Text")
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
        };

        var result = await ContentEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success, $"Failed to create content: {result.Status}");
        return result.Result.Content!;
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

    private async Task AssertTextValue(int propertyTypeId, string expected)
    {
        using IScope scope = ScopeProvider.CreateScope();
        var actual = await scope.Database.ExecuteScalarAsync<string>(
            "SELECT textValue FROM umbracoPropertyData WHERE propertyTypeId = @0",
            propertyTypeId);
        scope.Complete();
        Assert.AreEqual(expected, actual);
    }

    private async Task ExecuteMigration()
    {
        using IScope scope = ScopeProvider.CreateScope();
        await ConvertRichTextBlockUdisToKeys.ExecuteMigration(scope.Database);
        scope.Complete();
    }
}
