using System.Collections.Concurrent;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Infrastructure.HybridCache.Serialization;

internal sealed class MsgPackContentNestedDataSerializerFactory : IContentCacheDataSerializerFactory
{
    private readonly IPropertyCacheCompressionOptions _compressionOptions;
    private readonly IContentTypeService _contentTypeService;
    private readonly IMediaTypeService _mediaTypeService;
    private readonly PropertyEditorCollection _propertyEditors;
    private volatile TypeSnapshot _snapshot;

    public MsgPackContentNestedDataSerializerFactory(
        IContentTypeService contentTypeService,
        IMediaTypeService mediaTypeService,
        PropertyEditorCollection propertyEditors,
        IPropertyCacheCompressionOptions compressionOptions)
    {
        _contentTypeService = contentTypeService;
        _mediaTypeService = mediaTypeService;
        _propertyEditors = propertyEditors;
        _compressionOptions = compressionOptions;
        _snapshot = new TypeSnapshot(contentTypeService, mediaTypeService);
    }

    public void ClearTypeCache() => _snapshot = new TypeSnapshot(_contentTypeService, _mediaTypeService);

    public IContentCacheDataSerializer Create(ContentCacheDataSerializerEntityType types)
    {
        TypeSnapshot snapshot = _snapshot;

        // The type lookups are completed here, ahead of the content cache's own database queries, because querying
        // while those queries have open readers would fail.
        return snapshot.Serializers.GetOrAdd(
            types,
            static (types, state) => new Lazy<IContentCacheDataSerializer>(
                () => state.factory.CreateSerializer(types, state.snapshot),
                LazyThreadSafetyMode.PublicationOnly),
            (factory: this, snapshot)).Value;
    }

    private IContentCacheDataSerializer CreateSerializer(ContentCacheDataSerializerEntityType types, TypeSnapshot snapshot)
    {
        var contentTypes = new Dictionary<int, IContentTypeComposition>();
        if ((types & ContentCacheDataSerializerEntityType.Document) == ContentCacheDataSerializerEntityType.Document)
        {
            foreach (KeyValuePair<int, IContentTypeComposition> ct in snapshot.DocumentTypes.Value)
            {
                contentTypes[ct.Key] = ct.Value;
            }
        }

        if ((types & ContentCacheDataSerializerEntityType.Media) == ContentCacheDataSerializerEntityType.Media)
        {
            foreach (KeyValuePair<int, IContentTypeComposition> ct in snapshot.MediaTypes.Value)
            {
                contentTypes[ct.Key] = ct.Value;
            }
        }

        var compression =
            new PropertyCacheCompression(_compressionOptions, contentTypes, _propertyEditors, snapshot.IsCompressedCache);
        return new MsgPackContentNestedDataSerializer(compression);
    }

    /// <summary>
    ///     One reading of the content type and media type definitions, read on first use, together with what is derived
    ///     from it: the serializers and the memoised compression answers. Replaced as a whole when the definitions
    ///     change, so nothing derived from an earlier reading can outlive it.
    /// </summary>
    private sealed class TypeSnapshot
    {
        public TypeSnapshot(IContentTypeService contentTypeService, IMediaTypeService mediaTypeService)
        {
            DocumentTypes = new Lazy<IReadOnlyDictionary<int, IContentTypeComposition>>(
                () => contentTypeService.GetAll().ToDictionary(x => x.Id, x => (IContentTypeComposition)x),
                LazyThreadSafetyMode.PublicationOnly);
            MediaTypes = new Lazy<IReadOnlyDictionary<int, IContentTypeComposition>>(
                () => mediaTypeService.GetAll().ToDictionary(x => x.Id, x => (IContentTypeComposition)x),
                LazyThreadSafetyMode.PublicationOnly);
        }

        public Lazy<IReadOnlyDictionary<int, IContentTypeComposition>> DocumentTypes { get; }

        public Lazy<IReadOnlyDictionary<int, IContentTypeComposition>> MediaTypes { get; }

        public ConcurrentDictionary<ContentCacheDataSerializerEntityType, Lazy<IContentCacheDataSerializer>> Serializers { get; } = new();

        public ConcurrentDictionary<(int, string, bool), bool> IsCompressedCache { get; } = new();
    }
}
