using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Search.Indexing;
using Umbraco.Cms.Core.Search.Indexing.Collection;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Integration.Testing.Search;
using IndexValue = Umbraco.Cms.Core.Search.Indexing.IndexValue;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Search.Core;

/// <summary>
/// Verifies that the content of an externally referenced (reusable) block element is flattened into the referencing
/// document's index entry when <see cref="IndexingSettings.IndexExternalBlockElements"/> is enabled.
/// </summary>
public class ExternalBlockElementIndexingTests : PropertyValueHandlerTestsBase
{
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
        builder.WithCollectionBuilder<PropertyValueHandlerCollectionBuilder>().Add<GatedPropertyValueHandler>();
    }

    [Test]
    public async Task Can_Flatten_Published_External_Element_Content_Into_Published_Index_Only()
    {
        var (contentType, elementType) = await SetupBlockListWithElementType();

        Guid elementKey = CreateAndPublishElement(elementType, "The external element text");

        Content content = CreatePageWithExternalBlockReference(contentType, elementKey);
        ContentService.Save(content);
        ContentService.Publish(content, ["*"]);

        TestIndexDocument publishedDocument = IndexerAndSearcher.Dump(IndexAliases.PublishedContent).Single();
        IndexValue? publishedValue = publishedDocument.Fields.FirstOrDefault(f => f.FieldName == "blocks")?.Value;
        Assert.That(publishedValue, Is.Not.Null);
        CollectionAssert.Contains(publishedValue.Texts, "The external element text");

        // external element content is only ever flattened into the published index, never the draft one
        TestIndexDocument draftDocument = IndexerAndSearcher.Dump(IndexAliases.DraftContent).Single();
        IndexValue? draftValue = draftDocument.Fields.FirstOrDefault(f => f.FieldName == "blocks")?.Value;
        Assert.That(draftValue, Is.Null);
    }

    [Test]
    public async Task Cannot_Flatten_Unpublished_External_Element_Content()
    {
        var (contentType, elementType) = await SetupBlockListWithElementType();

        // create, but do not publish, the referenced element
        Element element = new ElementBuilder()
            .WithContentType(elementType)
            .WithName("Unpublished reusable element")
            .Build();
        element.SetValue("textValue", "Should not be indexed");
        ElementService.Save(element);

        Content content = CreatePageWithExternalBlockReference(contentType, element.Key);
        ContentService.Save(content);
        ContentService.Publish(content, ["*"]);

        TestIndexDocument publishedDocument = IndexerAndSearcher.Dump(IndexAliases.PublishedContent).Single();
        IndexValue? publishedValue = publishedDocument.Fields.FirstOrDefault(f => f.FieldName == "blocks")?.Value;
        Assert.That(publishedValue, Is.Null);
    }

    [Test]
    public async Task Circular_External_Element_Reference_Does_Not_Cause_Stack_Overflow()
    {
        // an element type whose own "nestedBlocks" block list property can reference other elements of the same
        // type - this allows an element to (directly or transitively) reference itself.
        Guid elementTypeKey = Guid.NewGuid();

        var nestedBlockListDataType = new DataType(GetRequiredService<PropertyEditorCollection>()[Constants.PropertyEditors.Aliases.BlockList], GetRequiredService<IConfigurationEditorJsonSerializer>())
        {
            ConfigurationData = new Dictionary<string, object>
            {
                {
                    "blocks",
                    new BlockListConfiguration.BlockConfiguration[]
                    {
                        new() { ContentElementTypeKey = elementTypeKey }
                    }
                }
            },
            Name = "Nested Block List",
            DatabaseType = ValueStorageType.Ntext,
            ParentId = Constants.System.Root,
            CreateDate = DateTime.UtcNow
        };
        await GetRequiredService<IDataTypeService>().CreateAsync(nestedBlockListDataType, Constants.Security.SuperUserKey);

        IContentType elementType = new ContentTypeBuilder()
            .WithKey(elementTypeKey)
            .WithAlias("selfReferencingElement")
            .WithName("Self Referencing Element")
            .WithIsElement(true)
            .AddPropertyType()
            .WithAlias("textValue")
            .WithName("Text")
            .WithDataTypeId(Constants.DataTypes.Textbox)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.TextBox)
            .Done()
            .AddPropertyType()
            .WithAlias("nestedBlocks")
            .WithName("Nested Blocks")
            .WithDataTypeId(nestedBlockListDataType.Id)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.BlockList)
            .Done()
            .Build();
        await ContentTypeService.CreateAsync(elementType, Constants.Security.SuperUserKey);

        var pageBlockListDataType = new DataType(GetRequiredService<PropertyEditorCollection>()[Constants.PropertyEditors.Aliases.BlockList], GetRequiredService<IConfigurationEditorJsonSerializer>())
        {
            ConfigurationData = new Dictionary<string, object>
            {
                {
                    "blocks",
                    new BlockListConfiguration.BlockConfiguration[]
                    {
                        new() { ContentElementTypeKey = elementTypeKey }
                    }
                }
            },
            Name = "My Block List",
            DatabaseType = ValueStorageType.Ntext,
            ParentId = Constants.System.Root,
            CreateDate = DateTime.UtcNow
        };
        await GetRequiredService<IDataTypeService>().CreateAsync(pageBlockListDataType, Constants.Security.SuperUserKey);

        IContentType contentType = new ContentTypeBuilder()
            .WithAlias("pageWithSelfReferencingBlocks")
            .WithName("Page With Self Referencing Blocks")
            .AddPropertyType()
            .WithAlias("blocks")
            .WithName("blocks")
            .WithDataTypeId(pageBlockListDataType.Id)
            .Done()
            .Build();
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        // the element references itself via its own "nestedBlocks" property
        Element element = new ElementBuilder()
            .WithContentType(elementType)
            .WithName("Self Referencing Element")
            .Build();
        element.SetValue("textValue", "Some element text");

        var selfReferencingBlockListValue = new BlockListValue
        {
            Layout = new Dictionary<string, IEnumerable<IBlockLayoutItem>>
            {
                {
                    Constants.PropertyEditors.Aliases.BlockList,
                    [new BlockListLayoutItem { ContentKey = element.Key, IsExternalContent = true }]
                }
            },
            ContentData = [],
            Expose = [],
        };
        element.SetValue("nestedBlocks", GetRequiredService<IJsonSerializer>().Serialize(selfReferencingBlockListValue));

        ElementService.Save(element);
        ElementService.Publish(element, ["*"]);

        Content content = CreatePageWithExternalBlockReference(contentType, element.Key);
        ContentService.Save(content);

        // this must not recurse indefinitely (and crash the process with a stack overflow) when flattening the
        // circular external element reference into the index
        Assert.DoesNotThrow(() => ContentService.Publish(content, ["*"]));

        TestIndexDocument publishedDocument = IndexerAndSearcher.Dump(IndexAliases.PublishedContent).Single();
        IndexValue? publishedValue = publishedDocument.Fields.FirstOrDefault(f => f.FieldName == "blocks")?.Value;
        Assert.That(publishedValue, Is.Not.Null);
        CollectionAssert.Contains(publishedValue.Texts, "Some element text");
    }

    [Test]
    public async Task Can_Flatten_Same_External_Element_Into_Concurrently_Indexed_Documents()
    {
        var (contentType, elementType) = await SetupBlockListWithElementType();
        IDataType textstringDataType = (await GetRequiredService<IDataTypeService>().GetAsync(Constants.DataTypes.Guids.TextstringGuid))!;
        elementType.AddPropertyType(new PropertyType(ShortStringHelper, textstringDataType, GatedPropertyValueHandler.PropertyAlias));
        await ContentTypeService.UpdateAsync(elementType, Constants.Security.SuperUserKey);

        Element element = new ElementBuilder()
            .WithContentType(elementType)
            .WithName("Shared element")
            .Build();
        element.SetValue("textValue", "The shared element text");
        element.SetValue(GatedPropertyValueHandler.PropertyAlias, "Gated text");
        ElementService.Save(element);
        ElementService.Publish(element, ["*"]);

        Content firstDocument = CreatePageWithExternalBlockReference(contentType, element.Key);
        ContentService.Save(firstDocument);
        ContentService.Publish(firstDocument, ["*"]);

        Content secondDocument = CreatePageWithExternalBlockReference(contentType, element.Key);
        ContentService.Save(secondDocument);
        ContentService.Publish(secondDocument, ["*"]);

        IProperty firstBlocksProperty = firstDocument.Properties["blocks"]!;
        IProperty secondBlocksProperty = secondDocument.Properties["blocks"]!;
        IPropertyValueHandler handler = GetRequiredService<PropertyValueHandlerCollection>().GetPropertyValueHandler(firstBlocksProperty.PropertyType)!;

        // a synchronous caller indexes once before starting concurrent work from the same execution context
        handler.GetIndexFields(firstBlocksProperty, null, null, true, firstDocument).ToArray();

        GatedPropertyValueHandler gate = GetRequiredService<GatedPropertyValueHandler>();
        gate.Arm();

        // the first flow holds the shared element in its ancestry chain until the gate opens
        Task<IndexField[]> firstFlow = Task.Run(() => handler.GetIndexFields(firstBlocksProperty, null, null, true, firstDocument).ToArray());
        Assert.That(gate.WaitUntilEntered(), Is.True, "The first flow never reached the shared element.");

        IndexField[] secondFlowFields;
        try
        {
            secondFlowFields = await Task.Run(() => handler.GetIndexFields(secondBlocksProperty, null, null, true, secondDocument).ToArray()).ConfigureAwait(false);
        }
        finally
        {
            gate.Open();
        }

        IndexField[] firstFlowFields = await firstFlow.ConfigureAwait(false);

        Assert.Multiple(() =>
        {
            CollectionAssert.Contains(firstFlowFields.SelectMany(field => field.Value.Texts ?? []), "The shared element text");
            CollectionAssert.Contains(secondFlowFields.SelectMany(field => field.Value.Texts ?? []), "The shared element text");
        });
    }

    private Guid CreateAndPublishElement(IContentType elementType, string textValue)
    {
        Element element = new ElementBuilder()
            .WithContentType(elementType)
            .WithName("Reusable element")
            .Build();
        element.SetValue("textValue", textValue);
        ElementService.Save(element);
        ElementService.Publish(element, ["*"]);
        return element.Key;
    }

    // once armed, blocks the first caller until opened, so a test can hold an indexing flow at a known point while
    // another flow runs. Only handles its own dedicated property alias, so it is inert for every other property.
    [HideFromTypeFinder]
    public sealed class GatedPropertyValueHandler : IPropertyValueHandler
    {
        public const string PropertyAlias = "gatedText";

        private static readonly TimeSpan _timeout = TimeSpan.FromSeconds(30);

        private readonly ManualResetEventSlim _entered = new();
        private readonly ManualResetEventSlim _opened = new();
        private int _state;

        public bool CanHandle(IPropertyType propertyType) => propertyType.Alias == PropertyAlias;

        public IEnumerable<IndexField> GetIndexFields(IProperty property, string? culture, string? segment, bool published, IContentBase contentContext)
        {
            if (Interlocked.CompareExchange(ref _state, 2, 1) == 1)
            {
                _entered.Set();
                _opened.Wait(_timeout);
            }

            return [new IndexField(property.Alias, new IndexValue { Texts = ["Gated text"] }, culture, segment)];
        }

        public void Arm() => Interlocked.Exchange(ref _state, 1);

        public bool WaitUntilEntered() => _entered.Wait(_timeout);

        public void Open() => _opened.Set();
    }
}
