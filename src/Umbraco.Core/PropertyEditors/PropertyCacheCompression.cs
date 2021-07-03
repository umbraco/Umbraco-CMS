using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models;

namespace Umbraco.Core.PropertyEditors
{

    /// <summary>
    /// Compresses property data based on config
    /// </summary>
    internal class PropertyCacheCompression : IPropertyCacheCompression
    {
        private readonly IPropertyCacheCompressionOptions _compressionOptions;
        private readonly IReadOnlyDictionary<int, IContentTypeComposition> _contentTypes;
        private readonly PropertyEditorCollection _propertyEditors;
        private readonly ConcurrentDictionary<(int contentTypeId, string propertyAlias), bool> _isCompressedCache;

        public PropertyCacheCompression(
            IPropertyCacheCompressionOptions compressionOptions,
            IReadOnlyDictionary<int, IContentTypeComposition> contentTypes,
            PropertyEditorCollection propertyEditors,
            ConcurrentDictionary<(int, string), bool> compressedStoragePropertyEditorCache)
        {
            _compressionOptions = compressionOptions;
            _contentTypes = contentTypes ?? throw new System.ArgumentNullException(nameof(contentTypes));
            _propertyEditors = propertyEditors ?? throw new System.ArgumentNullException(nameof(propertyEditors));
            _isCompressedCache = compressedStoragePropertyEditorCache;
        }

        public bool IsCompressed(IReadOnlyContentBase content, string alias)
        {
            var compressedStorage = _isCompressedCache.GetOrAdd((content.ContentTypeId, alias), x =>
            {
                if (!_contentTypes.TryGetValue(x.contentTypeId, out var ct))
                    return false;

                var propertyType = ct.CompositionPropertyTypes.FirstOrDefault(x => x.Alias == alias);
                if (propertyType == null) return false;

                if (!_propertyEditors.TryGet(propertyType.PropertyEditorAlias, out var propertyEditor)) return false;

                return _compressionOptions.IsCompressed(content, propertyType, propertyEditor);
            });

            return compressedStorage;
        }
    }
}
