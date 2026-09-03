using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence.Relations;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Integration.Testing.Search;
using IndexValue = Umbraco.Cms.Search.Core.Models.Indexing.IndexValue;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Search.Core;

/// <summary>
/// Verifies that a document referencing an external (reusable) block element is re-indexed when that element
/// itself changes, so its flattened content stays in sync.
/// </summary>
public class ElementIndexingNotificationHandlerTests : PropertyValueHandlerTestsBase
{
    private IJsonSerializer JsonSerializer => GetRequiredService<IJsonSerializer>();

    private IConfigurationEditorJsonSerializer ConfigurationEditorJsonSerializer => GetRequiredService<IConfigurationEditorJsonSerializer>();

    private PropertyEditorCollection PropertyEditorCollection => GetRequiredService<PropertyEditorCollection>();

    private IElementService ElementService => GetRequiredService<IElementService>();

    [SetUp]
    public void SetUp() => IndexerAndSearcher.Reset();

    protected override void ConfigureTestServices(IServiceCollection services)
    {
        base.ConfigureTestServices(services);
        services.Configure<IndexingSettings>(options => options.IndexExternalBlockElements = true);
    }

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        base.CustomTestSetup(builder);

        // creates the umbExternalBlockElement relation when the referencing document is saved/published
        builder
            .AddNotificationHandler<ContentSavedNotification, ContentRelationsUpdate>()
            .AddNotificationHandler<ContentPublishedNotification, ContentRelationsUpdate>();

        // broadcasts ElementCacheRefresherNotification (picked up by ElementIndexingNotificationHandler) when an
        // element is saved/published/unpublished/deleted
        builder.AddNotificationHandler<ElementTreeChangeNotification, ElementTreeChangeDistributedCacheNotificationHandler>();
    }

    [Test]
    public async Task Republishing_ReferencedElement_Reindexes_ReferencingDocument()
    {
        var (contentType, elementType) = await SetupBlockListWithElementType();

        Element element = new ElementBuilder()
            .WithContentType(elementType)
            .WithName("Reusable element")
            .Build();
        element.SetValue("textValue", "Original text");
        ElementService.Save(element);
        ElementService.Publish(element, ["*"]);

        Content content = CreatePageWithExternalBlockReference(contentType, element.Key);
        ContentService.Save(content);
        ContentService.Publish(content, ["*"]);

        AssertPublishedBlocksText("Original text");

        // change the element's content and republish - the referencing document should be transparently reindexed
        IElement updatedElement = ElementService.GetById(element.Key)!;
        updatedElement.SetValue("textValue", "Updated text");
        ElementService.Save(updatedElement);
        ElementService.Publish(updatedElement, ["*"]);

        AssertPublishedBlocksText("Updated text");

        return;

        void AssertPublishedBlocksText(string expectedText)
        {
            TestIndexDocument publishedDocument = IndexerAndSearcher.Dump(IndexAliases.PublishedContent).Single();
            IndexValue? publishedValue = publishedDocument.Fields.FirstOrDefault(f => f.FieldName == "blocks")?.Value;
            Assert.That(publishedValue, Is.Not.Null);
            CollectionAssert.Contains(publishedValue.Texts, expectedText);
        }
    }

    [Test]
    public async Task Unpublishing_ReferencedElement_Removes_ItsContent_From_ReferencingDocument()
    {
        var (contentType, elementType) = await SetupBlockListWithElementType();

        Element element = new ElementBuilder()
            .WithContentType(elementType)
            .WithName("Reusable element")
            .Build();
        element.SetValue("textValue", "Original text");
        ElementService.Save(element);
        ElementService.Publish(element, ["*"]);

        Content content = CreatePageWithExternalBlockReference(contentType, element.Key);
        ContentService.Save(content);
        ContentService.Publish(content, ["*"]);

        TestIndexDocument publishedDocumentBefore = IndexerAndSearcher.Dump(IndexAliases.PublishedContent).Single();
        IndexValue? publishedValueBefore = publishedDocumentBefore.Fields.FirstOrDefault(f => f.FieldName == "blocks")?.Value;
        Assert.That(publishedValueBefore, Is.Not.Null);

        ElementService.Unpublish(ElementService.GetById(element.Key)!);

        TestIndexDocument publishedDocumentAfter = IndexerAndSearcher.Dump(IndexAliases.PublishedContent).Single();
        IndexValue? publishedValueAfter = publishedDocumentAfter.Fields.FirstOrDefault(f => f.FieldName == "blocks")?.Value;
        Assert.That(publishedValueAfter, Is.Null);
    }

    private Content CreatePageWithExternalBlockReference(IContentType contentType, Guid externalElementKey)
    {
        var blockListValue = new BlockListValue
        {
            Layout = new Dictionary<string, IEnumerable<IBlockLayoutItem>>
            {
                {
                    Constants.PropertyEditors.Aliases.BlockList,
                    [new BlockListLayoutItem { ContentKey = externalElementKey, IsExternalContent = true }]
                }
            },
            ContentData = [],
            Expose = [],
        };

        Content content = new ContentBuilder()
            .WithContentType(contentType)
            .WithName("My Page")
            .Build();
        content.Properties["blocks"]!.SetValue(JsonSerializer.Serialize(blockListValue));
        return content;
    }

    private async Task<(IContentType ContentType, IContentType ElementType)> SetupBlockListWithElementType()
    {
        IContentType elementType = new ContentTypeBuilder()
            .WithAlias("reusableElement")
            .WithName("Reusable Element")
            .WithIsElement(true)
            .AddPropertyType()
            .WithAlias("textValue")
            .WithName("Text")
            .WithDataTypeId(Constants.DataTypes.Textbox)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.TextBox)
            .Done()
            .Build();
        await ContentTypeService.CreateAsync(elementType, Constants.Security.SuperUserKey);

        var blockListDataType = new DataType(PropertyEditorCollection[Constants.PropertyEditors.Aliases.BlockList], ConfigurationEditorJsonSerializer)
        {
            ConfigurationData = new Dictionary<string, object>
            {
                {
                    "blocks",
                    new BlockListConfiguration.BlockConfiguration[]
                    {
                        new() { ContentElementTypeKey = elementType.Key }
                    }
                }
            },
            Name = "My Block List",
            DatabaseType = ValueStorageType.Ntext,
            ParentId = Constants.System.Root,
            CreateDate = DateTime.UtcNow
        };
        await GetRequiredService<IDataTypeService>().CreateAsync(blockListDataType, Constants.Security.SuperUserKey);

        IContentType contentType = new ContentTypeBuilder()
            .WithAlias("pageWithBlocks")
            .WithName("Page With Blocks")
            .AddPropertyType()
            .WithAlias("blocks")
            .WithName("blocks")
            .WithDataTypeId(blockListDataType.Id)
            .Done()
            .Build();
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        return (contentType, elementType);
    }
}
