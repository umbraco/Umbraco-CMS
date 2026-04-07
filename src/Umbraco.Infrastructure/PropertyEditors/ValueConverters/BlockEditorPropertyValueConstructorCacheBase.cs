using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

/// <summary>
/// Serves as a base class that provides caching functionality for constructors used by block editor property value converters.
/// This helps optimize the instantiation of value converter objects in block editor scenarios.
/// </summary>
public abstract class BlockEditorPropertyValueConstructorCacheBase<T>
    where T : IBlockReference<IPublishedElement, IPublishedElement>
{
    private readonly
        ConcurrentDictionary<(Guid, Guid?), Func<Guid, IPublishedElement, Guid?, IPublishedElement?, T>>
        _constructorCache = new();

    /// <summary>
    /// Attempts to retrieve a cached constructor delegate for the specified content and optional settings type keys.
    /// </summary>
    /// <param name="key">A tuple consisting of the content type key (<see cref="Guid"/>) and an optional settings type key (<see cref="Guid?"/>).</param>
    /// <param name="value">When this method returns, contains the cached constructor delegate if found; otherwise, <c>null</c>.</param>
    /// <returns><c>true</c> if the constructor delegate was found in the cache; otherwise, <c>false</c>.</returns>
    public bool TryGetValue((Guid ContentTypeKey, Guid? SettingsTypeKey) key, [MaybeNullWhen(false)] out Func<Guid, IPublishedElement, Guid?, IPublishedElement?, T> value)
        => _constructorCache.TryGetValue(key, out value);

    /// <summary>
    /// Sets the value factory function for the specified content type and optional settings type key in the constructor cache.
    /// </summary>
    /// <param name="key">A tuple containing the content type key and an optional settings type key used to identify the cache entry.</param>
    /// <param name="value">A factory function that takes a content type key, a published element, an optional settings type key, and an optional published element, and returns a value of type <typeparamref name="T"/>.</param>
    public void SetValue((Guid ContentTypeKey, Guid? SettingsTypeKey) key, Func<Guid, IPublishedElement, Guid?, IPublishedElement?, T> value)
        => _constructorCache[key] = value;

    /// <summary>
    /// Clears the cache of constructor delegates for block editor property values.
    /// </summary>
    public void Clear()
        => _constructorCache.Clear();
}
