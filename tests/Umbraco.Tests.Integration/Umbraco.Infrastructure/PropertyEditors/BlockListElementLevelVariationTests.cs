using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Changes;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Tests.Common;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.PropertyEditors;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
public partial class BlockListElementLevelVariationTests : UmbracoIntegrationTest
{
    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private IContentService ContentService => GetRequiredService<IContentService>();

    private IDataTypeService DataTypeService => GetRequiredService<IDataTypeService>();

    private IJsonSerializer JsonSerializer => GetRequiredService<IJsonSerializer>();

    private PropertyEditorCollection PropertyEditorCollection => GetRequiredService<PropertyEditorCollection>();

    private IConfigurationEditorJsonSerializer ConfigurationEditorJsonSerializer => GetRequiredService<IConfigurationEditorJsonSerializer>();

    private IUmbracoContextFactory UmbracoContextFactory => GetRequiredService<IUmbracoContextFactory>();

    private IVariationContextAccessor VariationContextAccessor => GetRequiredService<IVariationContextAccessor>();

    private ILanguageService LanguageService => GetRequiredService<ILanguageService>();

    private IPublishedSnapshotService PublishedSnapshotService => GetRequiredService<IPublishedSnapshotService>();

    private IUmbracoContextAccessor UmbracoContextAccessor => GetRequiredService<IUmbracoContextAccessor>();

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        builder.Services.AddUnique<IUmbracoContextAccessor, TestUmbracoContextAccessor>();
        builder.Services.AddUnique(CreateMockHttpContextAccessor().Object);
        builder.AddNuCache();
    }

    [SetUp]
    public async Task SetUp() => await GetRequiredService<ILanguageService>().CreateAsync(
        new Language("da-DK", "Danish"), Constants.Security.SuperUserKey);

    private IContentType CreateElementType(ContentVariation variation)
    {
        var elementType = new ContentTypeBuilder()
            .WithAlias("myElementType")
            .WithName("My Element Type")
            .WithIsElement(true)
            .WithContentVariation(variation)
            .AddPropertyType()
            .WithAlias("invariantText")
            .WithName("Invariant text")
            .WithDataTypeId(Constants.DataTypes.Textbox)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.TextBox)
            .WithValueStorageType(ValueStorageType.Nvarchar)
            .WithVariations(ContentVariation.Nothing)
            .Done()
            .AddPropertyType()
            .WithAlias("variantText")
            .WithName("Variant text")
            .WithDataTypeId(Constants.DataTypes.Textbox)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.TextBox)
            .WithValueStorageType(ValueStorageType.Nvarchar)
            .WithVariations(variation)
            .Done()
            .Build();
        ContentTypeService.Save(elementType);
        return elementType;
    }

    private async Task<IDataType> CreateBlockListDataType(IContentType elementType)
    {
        var dataType = new DataType(PropertyEditorCollection[Constants.PropertyEditors.Aliases.BlockList], ConfigurationEditorJsonSerializer)
        {
            ConfigurationData = new Dictionary<string, object>
            {
                {
                    "blocks",
                    new BlockListConfiguration.BlockConfiguration[]
                    {
                        new() { ContentElementTypeKey = elementType.Key, SettingsElementTypeKey = elementType.Key }
                    }
                }
            },
            Name = "My Block List",
            DatabaseType = ValueStorageType.Ntext,
            ParentId = Constants.System.Root,
            CreateDate = DateTime.UtcNow
        };

        await DataTypeService.CreateAsync(dataType, Constants.Security.SuperUserKey);
        return dataType;
    }

    private IContentType CreateContentType(ContentVariation variation, IDataType blockListDataType, ContentVariation blocksVariation = ContentVariation.Nothing)
    {
        var contentType = new ContentTypeBuilder()
            .WithAlias("myPage")
            .WithName("My Page")
            .WithContentVariation(variation)
            .AddPropertyType()
            .WithAlias("blocks")
            .WithName("Blocks")
            .WithDataTypeId(blockListDataType.Id)
            .WithVariations(blocksVariation)
            .Done()
            .Build();
        ContentTypeService.Save(contentType);
        return contentType;
    }

    private IContent CreateContent(IContentType contentType, IContentType elementType, IList<BlockPropertyValue> blockContentValues, IList<BlockPropertyValue> blockSettingsValues, bool publishContent)
        => CreateContent(
            contentType,
            elementType,
            new[] { new BlockProperty(blockContentValues, blockSettingsValues, null, null) },
            publishContent);

    private IContent CreateContent(IContentType contentType, IContentType elementType, IEnumerable<BlockProperty> blocksProperties, bool publishContent)
    {
        var contentBuilder = new ContentBuilder()
            .WithContentType(contentType);
        contentBuilder = contentType.VariesByCulture()
            ? contentBuilder
                .WithCultureName("en-US", "Home (en)")
                .WithCultureName("da-DK", "Home (da)")
            : contentBuilder.WithName("Home");

        var content = contentBuilder.Build();

        var contentElementKey = Guid.NewGuid();
        var settingsElementKey = Guid.NewGuid();
        foreach (var blocksProperty in blocksProperties)
        {
            var blockListValue = BlockListPropertyValue(elementType, contentElementKey, settingsElementKey, blocksProperty);
            var propertyValue = JsonSerializer.Serialize(blockListValue);
            content.Properties["blocks"]!.SetValue(propertyValue, blocksProperty.Culture, blocksProperty.Segment);
        }

        ContentService.Save(content);

        if (publishContent)
        {
            PublishContent(content, contentType);
        }

        return content;
    }

    private BlockListValue BlockListPropertyValue(IContentType elementType, Guid contentElementKey, Guid settingsElementKey, BlockProperty blocksProperty)
        => BlockListPropertyValue(elementType, [(contentElementKey, settingsElementKey, blocksProperty)]);

    private BlockListValue BlockListPropertyValue(IContentType elementType, List<(Guid contentElementKey, Guid settingsElementKey, BlockProperty BlocksProperty)> blocks)
        => new()
        {
            Layout = new Dictionary<string, IEnumerable<IBlockLayoutItem>>
            {
                {
                    Constants.PropertyEditors.Aliases.BlockList,
                    blocks.Select(block => new BlockListLayoutItem
                    {
                        ContentKey = block.contentElementKey,
                        SettingsKey = block.settingsElementKey
                    }).ToArray()
                }
            },
            ContentData = blocks.Select(block => new BlockItemData
            {
                Key = block.contentElementKey,
                ContentTypeAlias = elementType.Alias,
                ContentTypeKey = elementType.Key,
                Values = block.BlocksProperty.BlockContentValues
            }).ToList(),
            SettingsData = blocks.Select(block => new BlockItemData
            {
                Key = block.settingsElementKey,
                ContentTypeAlias = elementType.Alias,
                ContentTypeKey = elementType.Key,
                Values = block.BlocksProperty.BlockSettingsValues
            }).ToList()
        };

    private void PublishContent(IContent content, IContentType contentType, string[]? culturesToPublish = null)
    {
        culturesToPublish ??= contentType.VariesByCulture()
            ? new[] { "en-US", "da-DK" }
            : new[] { "*" };
        var publishResult = ContentService.Publish(content, culturesToPublish);
        Assert.IsTrue(publishResult.Success);

        ContentCacheRefresher.JsonPayload[] payloads =
        [
            new ContentCacheRefresher.JsonPayload
            {
                ChangeTypes = TreeChangeTypes.RefreshNode,
                Key = content.Key,
                Id = content.Id,
                Blueprint = false
            }
        ];

        PublishedSnapshotService.Notify(payloads, out _, out _);
    }

    private async Task<IPublishedContent> CreatePublishedContent(ContentVariation variation, IList<BlockPropertyValue> blockContentValues, IList<BlockPropertyValue> blockSettingsValues)
    {
        var elementType = CreateElementType(variation);
        var blockListDataType = await CreateBlockListDataType(elementType);
        var contentType = CreateContentType(variation, blockListDataType);

        var content = CreateContent(contentType, elementType, blockContentValues, blockSettingsValues, true);
        return GetPublishedContent(content.Key);
    }

    private IPublishedContent GetPublishedContent(Guid key)
    {
        UmbracoContextAccessor.Clear();
        var umbracoContext = UmbracoContextFactory.EnsureUmbracoContext().UmbracoContext;
        var publishedContent = umbracoContext.Content?.GetById(key);
        Assert.IsNotNull(publishedContent);

        return publishedContent;
    }

    private void RefreshContentTypeCache(params IContentType[] contentTypes)
    {
        ContentTypeCacheRefresher.JsonPayload[] payloads = contentTypes
            .Select(contentType => new ContentTypeCacheRefresher.JsonPayload(nameof(IContentType), contentType.Id, ContentTypeChangeTypes.RefreshMain))
            .ToArray();

        PublishedSnapshotService.Notify(payloads);
    }

    private static Mock<IHttpContextAccessor> CreateMockHttpContextAccessor()
    {
        var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Scheme = "http";
        httpContext.Request.Host = new HostString("localhost");

        mockHttpContextAccessor.SetupGet(x => x.HttpContext).Returns(httpContext);
        return mockHttpContextAccessor;
    }

    private class BlockProperty
    {
        public BlockProperty(IList<BlockPropertyValue> blockContentValues, IList<BlockPropertyValue> blockSettingsValues, string? culture, string? segment)
        {
            BlockContentValues = blockContentValues;
            BlockSettingsValues = blockSettingsValues;
            Culture = culture;
            Segment = segment;
        }

        public IList<BlockPropertyValue> BlockContentValues { get; }

        public IList<BlockPropertyValue> BlockSettingsValues { get; }

        public string? Culture { get; }

        public string? Segment { get; }
    }
}
