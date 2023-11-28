using System.Diagnostics.CodeAnalysis;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

public abstract class BlockEditorPropertyValueConstructorCacheBase<T>
    where T : IBlockReference<IPublishedElement, IPublishedElement>
{
    private readonly
        Dictionary<(Guid, Guid?), Func<Udi, IPublishedElement, Udi?, IPublishedElement?, T>>
        _constructorCache = new();

    public bool TryGetValue((Guid ContentTypeKey, Guid? SettingsTypeKey) key, [MaybeNullWhen(false)] out Func<Udi, IPublishedElement, Udi?, IPublishedElement?, T> value)
        => _constructorCache.TryGetValue(key, out value);

    public void SetValue((Guid ContentTypeKey, Guid? SettingsTypeKey) key, Func<Udi, IPublishedElement, Udi?, IPublishedElement?, T> value)
        => _constructorCache[key] = value;

    public void Clear()
        => _constructorCache.Clear();
}
