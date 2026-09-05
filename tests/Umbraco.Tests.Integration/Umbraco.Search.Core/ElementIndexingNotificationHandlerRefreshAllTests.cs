using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Changes;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Infrastructure.Persistence.Relations;
using Umbraco.Cms.Search.Core.NotificationHandlers;
using Umbraco.Cms.Search.Core.Services.ContentIndexing;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Integration.Testing.Search;
using IndexValue = Umbraco.Cms.Search.Core.Models.Indexing.IndexValue;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Search.Core;

/// <summary>
/// Verifies that a "refresh all" element cache payload (carrying no specific element id - e.g. from
/// <see cref="Umbraco.Extensions.DistributedCacheExtensions.RefreshAllElementCache"/>) still causes every document
/// referencing an external element to be re-indexed, rather than being silently ignored.
/// </summary>
/// <remarks>
/// Deliberately does not register <see cref="ElementTreeChangeDistributedCacheNotificationHandler"/>, so that
/// saving/publishing the element does not itself broadcast a (per-element) <see cref="ElementCacheRefresherNotification"/>
/// - this isolates the "refresh all" code path from the already-covered per-element one.
/// </remarks>
public class ElementIndexingNotificationHandlerRefreshAllTests : PropertyValueHandlerTestsBase
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

        // creates the umbExternalBlockElement relation when the referencing document is saved/published
        builder
            .AddNotificationHandler<ContentSavedNotification, ContentRelationsUpdate>()
            .AddNotificationHandler<ContentPublishedNotification, ContentRelationsUpdate>();
    }

    [Test]
    public async Task Can_Reindex_Referencing_Document_When_Element_Cache_Is_Refreshed_Globally()
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

        AssertPublishedBlocksTextsContain("Original text");

        // change the element, but since ElementTreeChangeDistributedCacheNotificationHandler is not registered in
        // this fixture, nothing automatically triggers a re-index yet.
        IElement updatedElement = ElementService.GetById(element.Key)!;
        updatedElement.SetValue("textValue", "Updated text");
        ElementService.Save(updatedElement);
        ElementService.Publish(updatedElement, ["*"]);
        AssertPublishedBlocksTextsContain("Original text");

        // simulate a "refresh all" element cache payload - as broadcast by a full element cache reload - which
        // carries no specific element id.
        var refreshAllPayloads = new[] { new ElementCacheRefresher.JsonPayload(0, Guid.Empty, TreeChangeTypes.RefreshAll) };
        var notification = new ElementCacheRefresherNotification(refreshAllPayloads, MessageType.RefreshByPayload);

        var handler = new ElementIndexingNotificationHandler(
            GetRequiredService<ICoreScopeProvider>(),
            ContentIndexingService,
            GetRequiredService<IRelationService>(),
            GetRequiredService<IOptions<IndexingSettings>>(),
            GetRequiredService<IOriginProvider>());
        handler.Handle(notification);

        AssertPublishedBlocksTextsContain("Updated text");
    }

    private void AssertPublishedBlocksTextsContain(string expectedText)
    {
        TestIndexDocument publishedDocument = IndexerAndSearcher.Dump(IndexAliases.PublishedContent).Single();
        IndexValue? publishedValue = publishedDocument.Fields.FirstOrDefault(f => f.FieldName == "blocks")?.Value;
        Assert.That(publishedValue, Is.Not.Null);
        CollectionAssert.Contains(publishedValue.Texts, expectedText);
    }
}
