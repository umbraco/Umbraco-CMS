using K4os.Compression.LZ4;
using MessagePack;
using MessagePack.Resolvers;
using System;
using System.Linq;
using System.Text;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PublishedCache.NuCache.DataSource
{

    /// <summary>
    /// Serializes/Deserializes <see cref="ContentCacheDataModel"/> document to the SQL Database as bytes using MessagePack
    /// </summary>
    public class MsgPackContentNestedDataSerializer : IContentCacheDataSerializer
    {
        private readonly MessagePackSerializerOptions _options;
        private readonly IPropertyCompressionOptions _propertyOptions;

        public MsgPackContentNestedDataSerializer(IPropertyCompressionOptions propertyOptions)
        {
            _propertyOptions = propertyOptions ?? throw new ArgumentNullException(nameof(propertyOptions));

            var defaultOptions = ContractlessStandardResolver.Options;
            var resolver = CompositeResolver.Create(

                // TODO: We want to be able to intern the strings for aliases when deserializing like we do for Newtonsoft but I'm unsure exactly how
                // to do that but it would seem to be with a custom message pack resolver but I haven't quite figured out based on the docs how
                // to do that since that is part of the int key -> string mapping operation, might have to see the source code to figure that one out.

                // resolver custom types first
                // new ContentNestedDataResolver(),

                // finally use standard resolver
                defaultOptions.Resolver
            );

            _options = defaultOptions
                .WithResolver(resolver)
                .WithCompression(MessagePackCompression.Lz4BlockArray);            
        }

        public string ToJson(byte[] bin)
        {
            var json = MessagePackSerializer.ConvertToJson(bin, _options);
            return json;
        }

        public ContentCacheDataModel Deserialize(int contentTypeId, string stringData, byte[] byteData)
        {
            if (byteData != null)
            {
                var content = MessagePackSerializer.Deserialize<ContentCacheDataModel>(byteData, _options);
                Expand(contentTypeId, content);
                return content;
            }
            else if (stringData != null)
            {
                // NOTE: We don't really support strings but it's possible if manually used (i.e. tests)
                var bin = Convert.FromBase64String(stringData);
                var content = MessagePackSerializer.Deserialize<ContentCacheDataModel>(bin, _options);
                Expand(contentTypeId, content);
                return content;
            }
            else
            {
                return null;
            }
        }

        public ContentCacheDataSerializationResult Serialize(int contentTypeId, ContentCacheDataModel model)
        {
            Compress(contentTypeId, model);
            var bytes = MessagePackSerializer.Serialize(model, _options);
            return new ContentCacheDataSerializationResult(null, bytes);
        }

        /// <summary>
        /// Used during serialization to compress properties
        /// </summary>
        /// <param name="model"></param>
        /// <remarks>
        /// This will essentially 'double compress' property data. The MsgPack data as a whole will already be compressed
        /// but this will go a step further and double compress property data so that it is stored in the nucache file
        /// as compressed bytes and therefore will exist in memory as compressed bytes. That is, until the bytes are
        /// read/decompressed as a string to be displayed on the front-end. This allows for potentially a significant
        /// memory savings but could also affect performance of first rendering pages while decompression occurs.
        /// </remarks>
        private void Compress(int contentTypeId, ContentCacheDataModel model)
        {
            foreach(var propertyAliasToData in model.PropertyData)
            {
                if (_propertyOptions.IsCompressed(contentTypeId, propertyAliasToData.Key))
                {
                    foreach(var property in propertyAliasToData.Value.Where(x => x.Value != null && x.Value is string))
                    {
                        property.Value = LZ4Pickler.Pickle(Encoding.UTF8.GetBytes((string)property.Value), LZ4Level.L00_FAST);
                    }
                }
            }
        }

        /// <summary>
        /// Used during deserialization to map the property data as lazy or expand the value
        /// </summary>
        /// <param name="nestedData"></param>
        private void Expand(int contentTypeId, ContentCacheDataModel nestedData)
        {
            foreach (var propertyAliasToData in nestedData.PropertyData)
            {
                if (_propertyOptions.IsCompressed(contentTypeId, propertyAliasToData.Key))
                {
                    foreach (var property in propertyAliasToData.Value.Where(x => x.Value != null))
                    {
                        if (property.Value is byte[] byteArrayValue)
                        {
                            property.Value = new LazyCompressedString(byteArrayValue);
                        }
                    }
                }
            }
        }

       

        //private class ContentNestedDataResolver : IFormatterResolver
        //{
        //    // GetFormatter<T>'s get cost should be minimized so use type cache.
        //    public IMessagePackFormatter<T> GetFormatter<T>() => FormatterCache<T>.Formatter;

        //    private static class FormatterCache<T>
        //    {
        //        public static readonly IMessagePackFormatter<T> Formatter;

        //        // generic's static constructor should be minimized for reduce type generation size!
        //        // use outer helper method.
        //        static FormatterCache()
        //        {
        //            Formatter = (IMessagePackFormatter<T>)SampleCustomResolverGetFormatterHelper.GetFormatter(typeof(T));
        //        }
        //    }
        //}

        //internal static class SampleCustomResolverGetFormatterHelper
        //{
        //    // If type is concrete type, use type-formatter map
        //    static readonly Dictionary<Type, object> _formatterMap = new Dictionary<Type, object>()
        //    {
        //        {typeof(ContentNestedData), new ContentNestedDataFormatter()}
        //        // add more your own custom serializers.
        //    };

        //    internal static object GetFormatter(Type t)
        //    {
        //        object formatter;
        //        if (_formatterMap.TryGetValue(t, out formatter))
        //        {
        //            return formatter;
        //        }

        //        // If target type is generics, use MakeGenericType.
        //        if (t.IsGenericParameter && t.GetGenericTypeDefinition() == typeof(ValueTuple<,>))
        //        {
        //            return Activator.CreateInstance(typeof(ValueTupleFormatter<,>).MakeGenericType(t.GenericTypeArguments));
        //        }

        //        // If type can not get, must return null for fallback mechanism.
        //        return null;
        //    }
        //}

        //public class ContentNestedDataFormatter : IMessagePackFormatter<ContentNestedData>
        //{
        //    public void Serialize(ref MessagePackWriter writer, ContentNestedData value, MessagePackSerializerOptions options)
        //    {
        //        if (value == null)
        //        {
        //            writer.WriteNil();
        //            return;
        //        }

        //        writer.WriteArrayHeader(3);
        //        writer.WriteString(value.UrlSegment);
        //        writer.WriteString(value.FullName);
        //        writer.WriteString(value.Age);

        //        writer.WriteString(value.FullName);
        //    }

        //    public ContentNestedData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        //    {
        //        if (reader.TryReadNil())
        //        {
        //            return null;
        //        }

        //        options.Security.DepthStep(ref reader);

        //        var path = reader.ReadString();

        //        reader.Depth--;
        //        return new FileInfo(path);
        //    }
        //}
    }
}
