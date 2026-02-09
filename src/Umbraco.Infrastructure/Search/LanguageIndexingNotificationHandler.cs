using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Infrastructure.Examine;

namespace Umbraco.Cms.Infrastructure.Search;

public sealed class LanguageIndexingNotificationHandler :
    INotificationHandler<LanguageCacheRefresherNotification>,
    INotificationAsyncHandler<LanguageCacheRefresherNotification>
{
    private readonly IIndexRebuilder _indexRebuilder;
    private readonly IUmbracoIndexingHandler _umbracoIndexingHandler;

    public LanguageIndexingNotificationHandler(
        IUmbracoIndexingHandler umbracoIndexingHandler,
        IIndexRebuilder indexRebuilder)
    {
        _umbracoIndexingHandler =
            umbracoIndexingHandler ?? throw new ArgumentNullException(nameof(umbracoIndexingHandler));
        _indexRebuilder = indexRebuilder ?? throw new ArgumentNullException(nameof(indexRebuilder));
    }

    /// <inheritdoc />
    [Obsolete("Use HandleAsync instead. Scheduled for removal in V19.")]
    public void Handle(LanguageCacheRefresherNotification args)
        => HandleAsync(args, CancellationToken.None).GetAwaiter().GetResult();

    /// <inheritdoc />
    public async Task HandleAsync(LanguageCacheRefresherNotification notification, CancellationToken cancellationToken)
    {
        if (!_umbracoIndexingHandler.Enabled)
        {
            return;
        }

        if (notification.MessageObject is not LanguageCacheRefresher.JsonPayload[] payloads)
        {
            return;
        }

        if (payloads.Length == 0)
        {
            return;
        }

        var removedOrCultureChanged = payloads.Any(x =>
            x.ChangeType is LanguageCacheRefresher.JsonPayload.LanguageChangeType.ChangeCulture
                or LanguageCacheRefresher.JsonPayload.LanguageChangeType.Remove);

        if (removedOrCultureChanged)
        {
            // if a lang is removed or it's culture has changed, we need to rebuild the indexes since
            // field names and values in the index have a string culture value.
            _ = await _indexRebuilder.RebuildIndexesAsync(false);
        }
    }
}
