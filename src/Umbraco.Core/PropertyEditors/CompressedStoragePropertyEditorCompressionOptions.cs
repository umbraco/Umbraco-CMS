using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models;

namespace Umbraco.Core.PropertyEditors
{

    /// <summary>
    /// Ensures all property types that have a property editor attributed with <see cref="CompressedStorageAttribute"/> use data compression
    /// </summary>
    internal class CompressedStoragePropertyEditorCompressionOptions : IPropertyCompressionOptions
    {
        private readonly IReadOnlyDictionary<int, IContentTypeComposition> _contentTypes;
        private readonly PropertyEditorCollection _propertyEditors;
        private readonly ConcurrentDictionary<(int, string), CompressedStorageAttribute> _compressedStoragePropertyEditorCache;

        public CompressedStoragePropertyEditorCompressionOptions(
            IReadOnlyDictionary<int, IContentTypeComposition> contentTypes,
            PropertyEditorCollection propertyEditors,
            ConcurrentDictionary<(int, string), CompressedStorageAttribute> compressedStoragePropertyEditorCache)
        {
            _contentTypes = contentTypes ?? throw new System.ArgumentNullException(nameof(contentTypes));
            _propertyEditors = propertyEditors ?? throw new System.ArgumentNullException(nameof(propertyEditors));
            _compressedStoragePropertyEditorCache = compressedStoragePropertyEditorCache;
        }

        public bool IsCompressed(int contentTypeId, string alias)
        {
            var compressedStorage = _compressedStoragePropertyEditorCache.GetOrAdd((contentTypeId, alias), x =>
            {
                if (!_contentTypes.TryGetValue(contentTypeId, out var ct))
                    return null;

                var propertyType = ct.CompositionPropertyTypes.FirstOrDefault(x => x.Alias == alias);
                if (propertyType == null) return null;

                if (!_propertyEditors.TryGet(propertyType.PropertyEditorAlias, out var propertyEditor)) return null;

                var attribute = propertyEditor.GetType().GetCustomAttribute<CompressedStorageAttribute>(true);
                return attribute;
            });

            return compressedStorage?.IsCompressed ?? false;
        }
    }
}
