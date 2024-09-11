using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

public class ConstructorCacheClearNotificationHandler :
    INotificationHandler<ContentTypeCacheRefresherNotification>,
    INotificationHandler<DataTypeCacheRefresherNotification>
{
    private readonly BlockListPropertyValueConstructorCache _blockListPropertyValueConstructorCache;
    private readonly BlockGridPropertyValueConstructorCache _blockGridPropertyValueConstructorCache;
    private readonly RichTextBlockPropertyValueConstructorCache _richTextBlockPropertyValueConstructorCache;

    public ConstructorCacheClearNotificationHandler(
        BlockListPropertyValueConstructorCache blockListPropertyValueConstructorCache,
        BlockGridPropertyValueConstructorCache blockGridPropertyValueConstructorCache,
        RichTextBlockPropertyValueConstructorCache richTextBlockPropertyValueConstructorCache)
    {
        _blockListPropertyValueConstructorCache = blockListPropertyValueConstructorCache;
        _blockGridPropertyValueConstructorCache = blockGridPropertyValueConstructorCache;
        _richTextBlockPropertyValueConstructorCache = richTextBlockPropertyValueConstructorCache;
    }

    public void Handle(ContentTypeCacheRefresherNotification notification)
        => ClearCaches();

    public void Handle(DataTypeCacheRefresherNotification notification)
        => ClearCaches();

    private void ClearCaches()
    {
        // must clear the block item constructor caches whenever content types and data types change,
        // otherwise InMemoryAuto generated models will not work.
        _blockListPropertyValueConstructorCache.Clear();
        _blockGridPropertyValueConstructorCache.Clear();
        _richTextBlockPropertyValueConstructorCache.Clear();
    }
}
