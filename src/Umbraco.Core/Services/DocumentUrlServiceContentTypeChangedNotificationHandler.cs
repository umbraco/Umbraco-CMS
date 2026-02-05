using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services.Changes;

namespace Umbraco.Cms.Core.Services;

/// <summary>
/// Handles content type changes to rebuild URL and alias caches when content type variation changes.
/// This ensures that when a content type changes from variant to invariant (or vice versa),
/// the URL cache is properly rebuilt with the correct languageId (NULL for invariant, specific ID for variant).
/// </summary>
public class DocumentUrlServiceContentTypeChangedNotificationHandler : INotificationAsyncHandler<ContentTypeChangedNotification>
{
    private readonly IDocumentUrlService _documentUrlService;
    private readonly IDocumentUrlAliasService _documentUrlAliasService;
    private readonly IContentService _contentService;

    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentUrlServiceContentTypeChangedNotificationHandler"/> class.
    /// </summary>
    /// <param name="documentUrlService">The document URL service.</param>
    /// <param name="documentUrlAliasService">The document URL alias service.</param>
    /// <param name="contentService">The content service.</param>
    public DocumentUrlServiceContentTypeChangedNotificationHandler(
        IDocumentUrlService documentUrlService,
        IDocumentUrlAliasService documentUrlAliasService,
        IContentService contentService)
    {
        _documentUrlService = documentUrlService;
        _documentUrlAliasService = documentUrlAliasService;
        _contentService = contentService;
    }

    /// <inheritdoc/>
    public async Task HandleAsync(ContentTypeChangedNotification notification, CancellationToken cancellationToken)
    {
        // Only process if the service is initialized (skip during install/upgrade).
        if (_documentUrlService.IsInitialized is false)
        {
            return;
        }

        // Only process content types where variation has changed (e.g., from invariant to variant or vice versa).
        // This is the only change that affects how URL segments are stored (NULL languageId vs specific ID).
        IEnumerable<ContentTypeChange<IContentType>> variationChangedContentTypes = notification.Changes
            .Where(change => change.ChangeTypes.HasFlag(ContentTypeChangeTypes.VariationChanged));

        foreach (ContentTypeChange<IContentType> change in variationChangedContentTypes)
        {
            await RebuildUrlCacheForContentTypeAsync(change.Item.Id, cancellationToken);
        }
    }

    private async Task RebuildUrlCacheForContentTypeAsync(int contentTypeId, CancellationToken cancellationToken)
    {
        const int pageSize = 500;
        long pageIndex = 0;
        long totalRecords;

        do
        {
            IEnumerable<IContent> contentItems = _contentService.GetPagedOfTypes(
                [contentTypeId],
                pageIndex,
                pageSize,
                out totalRecords,
                filter: null,
                ordering: null);

            foreach (IContent content in contentItems)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                // Rebuild URL segments for this content item.
                await _documentUrlService.CreateOrUpdateUrlSegmentsAsync(content.Key);

                // Rebuild aliases for this content item.
                await _documentUrlAliasService.CreateOrUpdateAliasesAsync(content.Key);
            }

            pageIndex++;
        }
        while (pageIndex * pageSize < totalRecords);
    }
}
