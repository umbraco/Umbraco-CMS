using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Integration.Testing.Search;
using IndexValue = Umbraco.Cms.Search.Core.Models.Indexing.IndexValue;

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
}
