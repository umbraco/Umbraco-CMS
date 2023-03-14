using System.Collections.Concurrent;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Compresses property data based on config
/// </summary>
public class PropertyCacheCompression : IPropertyCacheCompression
{
    private readonly IPropertyCacheCompressionOptions _compressionOptions;
    private readonly IReadOnlyDictionary<int, IContentTypeComposition> _contentTypes;

    private readonly ConcurrentDictionary<(int contentTypeId, string propertyAlias, bool published), bool>
        _isCompressedCache;

    private readonly PropertyEditorCollection _propertyEditors;

    public PropertyCacheCompression(
        IPropertyCacheCompressionOptions compressionOptions,
        IReadOnlyDictionary<int, IContentTypeComposition> contentTypes,
        PropertyEditorCollection propertyEditors,
        ConcurrentDictionary<(int, string, bool), bool> compressedStoragePropertyEditorCache)
    {
        _compressionOptions = compressionOptions;
        _contentTypes = contentTypes ?? throw new ArgumentNullException(nameof(contentTypes));
        _propertyEditors = propertyEditors ?? throw new ArgumentNullException(nameof(propertyEditors));
        _isCompressedCache = compressedStoragePropertyEditorCache;
    }

    public bool IsCompressed(IReadOnlyContentBase content, string alias, bool published)
    {
        var compressedStorage = _isCompressedCache.GetOrAdd((content.ContentTypeId, alias, published), x =>
        {
            if (!_contentTypes.TryGetValue(x.contentTypeId, out IContentTypeComposition? ct))
            {
                return false;
            }

            IPropertyType? propertyType = ct.CompositionPropertyTypes.FirstOrDefault(x => x.Alias == alias);
            if (propertyType == null)
            {
                return false;
            }

            if (!_propertyEditors.TryGet(propertyType.PropertyEditorAlias, out IDataEditor? propertyEditor))
            {
                return false;
            }

            return _compressionOptions.IsCompressed(content, propertyType, propertyEditor, published);
        });

        return compressedStorage;
    }
}
