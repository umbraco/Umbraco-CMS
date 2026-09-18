using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence.Relations;
using Umbraco.Cms.Search.Core.Persistence;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Integration.Testing.Search;
using IndexValue = Umbraco.Cms.Search.Core.Models.Indexing.IndexValue;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Search.Core;

/// <summary>
/// Verifies that republishing an externally referenced (reusable) element refreshes the index entry of every
/// document that references it, even though the referencing document's own content is unchanged.
/// </summary>
public class ExternalElementReindexOnRepublishTests : PropertyValueHandlerTestsBase
{
    private IElementService ElementService => GetRequiredService<IElementService>();

    [SetUp]
    public void SetUp() => IndexerAndSearcher.Reset();

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        base.CustomTestSetup(builder);

        // creates the umbExternalBlockElement relation when the referencing document is saved/published, so the
        // element-change handler can find "My Page" as a referencing document.
        builder
            .AddNotificationHandler<ContentSavedNotification, ContentRelationsUpdate>()
            .AddNotificationHandler<ContentPublishedNotification, ContentRelationsUpdate>();
    }

    protected override void ConfigureTestServices(IServiceCollection services)
    {
        base.ConfigureTestServices(services);
        services.Configure<IndexingSettings>(options => options.IndexExternalBlockElements = true);

        // TestBase registers a no-op IIndexDocumentRepository, which bypasses the persisted change-detection
        // cache entirely and would hide the exact bug under test here - use the real, database-backed
        // repository instead so a stale cached snapshot can actually occur.
        services.AddSingleton<IIndexDocumentRepository, IndexDocumentRepository>();
    }

    [Test]
    public async Task Republishing_External_Element_Refreshes_Referencing_Document_Index()
    {
        var (contentType, elementType) = await SetupBlockListWithElementType();

        Element element = new ElementBuilder()
            .WithContentType(elementType)
            .WithName("Reusable element")
            .Build();
        element.SetValue("textValue", "Original element text");
        ElementService.Save(element);
        ElementService.Publish(element, ["*"]);

        Content content = CreatePageWithExternalBlockReference(contentType, element.Key);
        ContentService.Save(content);
        ContentService.Publish(content, ["*"]);

        TestIndexDocument initialDocument = IndexerAndSearcher.Dump(IndexAliases.PublishedContent).Single();
        IndexValue? initialValue = initialDocument.Fields.FirstOrDefault(f => f.FieldName == "blocks")?.Value;
        Assert.That(initialValue, Is.Not.Null);
        CollectionAssert.Contains(initialValue.Texts, "Original element text");

        // republish the already-referenced element with different text - the referencing document's own
        // content is untouched, so its index entry must be refreshed purely as a result of the element change.
        IElement publishedElement = ElementService.GetById(element.Key)!;
        publishedElement.SetValue("textValue", "Updated element text");
        ElementService.Save(publishedElement);
        ElementService.Publish(publishedElement, ["*"]);

        TestIndexDocument updatedDocument = IndexerAndSearcher.Dump(IndexAliases.PublishedContent).Single();
        IndexValue? updatedValue = updatedDocument.Fields.FirstOrDefault(f => f.FieldName == "blocks")?.Value;
        Assert.That(updatedValue, Is.Not.Null);
        CollectionAssert.Contains(updatedValue.Texts, "Updated element text");
        CollectionAssert.DoesNotContain(updatedValue.Texts, "Original element text");
    }
}
