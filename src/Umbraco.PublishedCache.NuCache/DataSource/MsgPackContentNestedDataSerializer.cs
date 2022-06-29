using System.Text;
using K4os.Compression.LZ4;
using MessagePack;
using MessagePack.Resolvers;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;

namespace Umbraco.Cms.Infrastructure.PublishedCache.DataSource;

/// <summary>
///     Serializes/Deserializes <see cref="ContentCacheDataModel" /> document to the SQL Database as bytes using
///     MessagePack
/// </summary>
public class MsgPackContentNestedDataSerializer : IContentCacheDataSerializer
{
    private readonly MessagePackSerializerOptions _options;
    private readonly IPropertyCacheCompression _propertyOptions;

    public MsgPackContentNestedDataSerializer(IPropertyCacheCompression propertyOptions)
    {
        _propertyOptions = propertyOptions ?? throw new ArgumentNullException(nameof(propertyOptions));

        MessagePackSerializerOptions? defaultOptions = ContractlessStandardResolver.Options;
        IFormatterResolver? resolver = CompositeResolver.Create(

            // TODO: We want to be able to intern the strings for aliases when deserializing like we do for Newtonsoft but I'm unsure exactly how
            // to do that but it would seem to be with a custom message pack resolver but I haven't quite figured out based on the docs how
            // to do that since that is part of the int key -> string mapping operation, might have to see the source code to figure that one out.
            // There are docs here on how to build one of these: https://github.com/neuecc/MessagePack-CSharp/blob/master/README.md#low-level-api-imessagepackformattert
            // and there are a couple examples if you search on google for them but this will need to be a separate project.
            // NOTE: resolver custom types first
            // new ContentNestedDataResolver(),

            // finally use standard resolver
            defaultOptions.Resolver);

        _options = defaultOptions
            .WithResolver(resolver)
            .WithCompression(MessagePackCompression.Lz4BlockArray)
            .WithSecurity(MessagePackSecurity.UntrustedData);
    }

    public ContentCacheDataModel? Deserialize(IReadOnlyContentBase content, string? stringData, byte[]? byteData, bool published)
    {
        if (byteData != null)
        {
            ContentCacheDataModel? cacheModel =
                MessagePackSerializer.Deserialize<ContentCacheDataModel>(byteData, _options);
            Expand(content, cacheModel, published);
            return cacheModel;
        }

        if (stringData != null)
        {
            // NOTE: We don't really support strings but it's possible if manually used (i.e. tests)
            var bin = Convert.FromBase64String(stringData);
            ContentCacheDataModel? cacheModel = MessagePackSerializer.Deserialize<ContentCacheDataModel>(bin, _options);
            Expand(content, cacheModel, published);
            return cacheModel;
        }

        return null;
    }

    public ContentCacheDataSerializationResult Serialize(IReadOnlyContentBase content, ContentCacheDataModel model, bool published)
    {
        Compress(content, model, published);
        var bytes = MessagePackSerializer.Serialize(model, _options);
        return new ContentCacheDataSerializationResult(null, bytes);
    }

    public string ToJson(byte[] bin)
    {
        var json = MessagePackSerializer.ConvertToJson(bin, _options);
        return json;
    }

    /// <summary>
    ///     Used during serialization to compress properties
    /// </summary>
    /// <param name="content"></param>
    /// <param name="model"></param>
    /// <param name="published"></param>
    /// <remarks>
    ///     This will essentially 'double compress' property data. The MsgPack data as a whole will already be compressed
    ///     but this will go a step further and double compress property data so that it is stored in the nucache file
    ///     as compressed bytes and therefore will exist in memory as compressed bytes. That is, until the bytes are
    ///     read/decompressed as a string to be displayed on the front-end. This allows for potentially a significant
    ///     memory savings but could also affect performance of first rendering pages while decompression occurs.
    /// </remarks>
    private void Compress(IReadOnlyContentBase content, ContentCacheDataModel model, bool published)
    {
        if (model.PropertyData is null)
        {
            return;
        }

        foreach (KeyValuePair<string, PropertyData[]> propertyAliasToData in model.PropertyData)
        {
            if (_propertyOptions.IsCompressed(content, propertyAliasToData.Key, published))
            {
                foreach (PropertyData property in propertyAliasToData.Value.Where(x =>
                             x.Value != null && x.Value is string))
                {
                    if (property.Value is string propertyValue)
                    {
                        property.Value = LZ4Pickler.Pickle(Encoding.UTF8.GetBytes(propertyValue));
                    }
                }

                foreach (PropertyData property in propertyAliasToData.Value.Where(x =>
                             x.Value != null && x.Value is int intVal))
                {
                    property.Value = Convert.ToBoolean((int?)property.Value);
                }
            }
        }
    }

    /// <summary>
    ///     Used during deserialization to map the property data as lazy or expand the value
    /// </summary>
    /// <param name="content"></param>
    /// <param name="nestedData"></param>
    /// <param name="published"></param>
    private void Expand(IReadOnlyContentBase content, ContentCacheDataModel nestedData, bool published)
    {
        if (nestedData.PropertyData is null)
        {
            return;
        }

        foreach (KeyValuePair<string, PropertyData[]> propertyAliasToData in nestedData.PropertyData)
        {
            if (_propertyOptions.IsCompressed(content, propertyAliasToData.Key, published))
            {
                foreach (PropertyData property in propertyAliasToData.Value.Where(x => x.Value != null))
                {
                    if (property.Value is byte[] byteArrayValue)
                    {
                        property.Value = new LazyCompressedString(byteArrayValue);
                    }
                }
            }
        }
    }
}
