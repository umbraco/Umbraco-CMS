using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Infrastructure.Examine;

namespace Umbraco.Cms.Infrastructure.Search;

public sealed class LanguageIndexingNotificationHandler : INotificationHandler<LanguageCacheRefresherNotification>
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

    public void Handle(LanguageCacheRefresherNotification args)
    {
        if (!_umbracoIndexingHandler.Enabled)
        {
            return;
        }

        if (!(args.MessageObject is LanguageCacheRefresher.JsonPayload[] payloads))
        {
            return;
        }

        if (payloads.Length == 0)
        {
            return;
        }

        var removedOrCultureChanged = payloads.Any(x =>
            x.ChangeType == LanguageCacheRefresher.JsonPayload.LanguageChangeType.ChangeCulture
            || x.ChangeType == LanguageCacheRefresher.JsonPayload.LanguageChangeType.Remove);

        if (removedOrCultureChanged)
        {
            // if a lang is removed or it's culture has changed, we need to rebuild the indexes since
            // field names and values in the index have a string culture value.
            _indexRebuilder.RebuildIndexes(false);
        }
    }
}
