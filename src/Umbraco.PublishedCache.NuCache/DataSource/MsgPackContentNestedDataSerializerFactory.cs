using System.Collections.Concurrent;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Infrastructure.PublishedCache.DataSource;

internal class MsgPackContentNestedDataSerializerFactory : IContentCacheDataSerializerFactory
{
    private readonly IPropertyCacheCompressionOptions _compressionOptions;
    private readonly IContentTypeService _contentTypeService;
    private readonly ConcurrentDictionary<(int, string, bool), bool> _isCompressedCache = new();
    private readonly IMediaTypeService _mediaTypeService;
    private readonly IMemberTypeService _memberTypeService;
    private readonly PropertyEditorCollection _propertyEditors;

    public MsgPackContentNestedDataSerializerFactory(
        IContentTypeService contentTypeService,
        IMediaTypeService mediaTypeService,
        IMemberTypeService memberTypeService,
        PropertyEditorCollection propertyEditors,
        IPropertyCacheCompressionOptions compressionOptions)
    {
        _contentTypeService = contentTypeService;
        _mediaTypeService = mediaTypeService;
        _memberTypeService = memberTypeService;
        _propertyEditors = propertyEditors;
        _compressionOptions = compressionOptions;
    }

    public IContentCacheDataSerializer Create(ContentCacheDataSerializerEntityType types)
    {
        // Depending on which entity types are being requested, we need to look up those content types
        // to initialize the compression options.
        // We need to initialize these options now so that any data lookups required are completed and are not done while the content cache
        // is performing DB queries which will result in errors since we'll be trying to query with open readers.
        // NOTE: The calls to GetAll() below should be cached if the data has not been changed.
        var contentTypes = new Dictionary<int, IContentTypeComposition>();
        if ((types & ContentCacheDataSerializerEntityType.Document) == ContentCacheDataSerializerEntityType.Document)
        {
            foreach (IContentType ct in _contentTypeService.GetAll())
            {
                contentTypes[ct.Id] = ct;
            }
        }

        if ((types & ContentCacheDataSerializerEntityType.Media) == ContentCacheDataSerializerEntityType.Media)
        {
            foreach (IMediaType ct in _mediaTypeService.GetAll())
            {
                contentTypes[ct.Id] = ct;
            }
        }

        if ((types & ContentCacheDataSerializerEntityType.Member) == ContentCacheDataSerializerEntityType.Member)
        {
            foreach (IMemberType ct in _memberTypeService.GetAll())
            {
                contentTypes[ct.Id] = ct;
            }
        }

        var compression =
            new PropertyCacheCompression(_compressionOptions, contentTypes, _propertyEditors, _isCompressedCache);
        var serializer = new MsgPackContentNestedDataSerializer(compression);

        return serializer;
    }
}
