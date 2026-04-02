using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

/// <summary>
/// Handles notifications that trigger the clearing of the constructor cache for property value converters.
/// </summary>
public class ConstructorCacheClearNotificationHandler :
    INotificationHandler<ContentTypeCacheRefresherNotification>,
    INotificationHandler<DataTypeCacheRefresherNotification>
{
    private readonly BlockListPropertyValueConstructorCache _blockListPropertyValueConstructorCache;
    private readonly BlockGridPropertyValueConstructorCache _blockGridPropertyValueConstructorCache;
    private readonly RichTextBlockPropertyValueConstructorCache _richTextBlockPropertyValueConstructorCache;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConstructorCacheClearNotificationHandler"/> class.
    /// </summary>
    /// <param name="blockListPropertyValueConstructorCache">Cache for block list property value constructors.</param>
    /// <param name="blockGridPropertyValueConstructorCache">Cache for block grid property value constructors.</param>
    /// <param name="richTextBlockPropertyValueConstructorCache">Cache for rich text block property value constructors.</param>
    public ConstructorCacheClearNotificationHandler(
        BlockListPropertyValueConstructorCache blockListPropertyValueConstructorCache,
        BlockGridPropertyValueConstructorCache blockGridPropertyValueConstructorCache,
        RichTextBlockPropertyValueConstructorCache richTextBlockPropertyValueConstructorCache)
    {
        _blockListPropertyValueConstructorCache = blockListPropertyValueConstructorCache;
        _blockGridPropertyValueConstructorCache = blockGridPropertyValueConstructorCache;
        _richTextBlockPropertyValueConstructorCache = richTextBlockPropertyValueConstructorCache;
    }

    /// <summary>
    /// Handles a notification to clear the constructor cache when content type changes occur.
    /// </summary>
    /// <param name="notification">The notification that triggers the cache clearing process.</param>
    public void Handle(ContentTypeCacheRefresherNotification notification)
        => ClearCaches();

    /// <summary>
    /// Handles a <see cref="DataTypeCacheRefresherNotification"/> to clear the constructor cache when data type changes occur.
    /// </summary>
    /// <param name="notification">The <see cref="DataTypeCacheRefresherNotification"/> that triggers the cache clearing process.</param>
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
