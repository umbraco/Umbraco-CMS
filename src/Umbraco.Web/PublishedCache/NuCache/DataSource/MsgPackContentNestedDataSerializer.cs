using K4os.Compression.LZ4;
using MessagePack;
using MessagePack.Formatters;
using MessagePack.Resolvers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Umbraco.Web.PublishedCache.NuCache.DataSource
{
    internal class MsgPackContentNestedDataSerializer : IContentNestedDataByteSerializer
    {
        private MessagePackSerializerOptions _options;
        private readonly NucachePropertyOptions _propertyOptions;

        public MsgPackContentNestedDataSerializer(INuCachePropertyOptionsFactory propertyOptionsFactory = null)
        {
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
            _propertyOptions = propertyOptionsFactory?.GetNuCachePropertyOptions() ?? new NucachePropertyOptions();
        }

        public string ToJson(string serialized)
        {
            var bin = Convert.FromBase64String(serialized);
            var json = MessagePackSerializer.ConvertToJson(bin, _options);
            return json;
        }

        public ContentNestedData Deserialize(string data)
        {
            var bin = Convert.FromBase64String(data);
            var obj = MessagePackSerializer.Deserialize<ContentNestedData>(bin, _options);
            return obj;
        }

        public string Serialize(ContentNestedData nestedData)
        {
            Optimize(nestedData);

            var bin = MessagePackSerializer.Serialize(nestedData, _options);
            return Convert.ToBase64String(bin);
        }

        /// <summary>
        /// Compress properties and map property names to shorter names
        /// </summary>
        /// <param name="nestedData"></param>
        private void Optimize(ContentNestedData nestedData)
        {
            if (_propertyOptions.PropertyMap != null && _propertyOptions.PropertyMap.Any())
            {
                foreach (var map in _propertyOptions.PropertyMap)
                {
                    if (map.Value.compress.Equals(NucachePropertyCompressionLevel.SQLDatabase))
                    {
                        if (nestedData.PropertyData.TryGetValue(map.Key, out PropertyData[] properties))
                        {
                            foreach (var property in properties.Where(x => x.Value != null && x.Value is string))
                            {
                                property.Value = LZ4Pickler.Pickle(Encoding.UTF8.GetBytes(property.Value as string), _propertyOptions.LZ4CompressionLevel);
                            }
                        }
                    }
                    if (map.Value.mappedAlias != null && !map.Key.Equals(map.Value.mappedAlias)
                        && nestedData.PropertyData.Remove(map.Key) && nestedData.PropertyData.TryGetValue(map.Key, out PropertyData[] properties2))
                    {
                        nestedData.PropertyData.Remove(map.Key);
                        nestedData.PropertyData.Add(map.Value.mappedAlias, properties2);
                    }
                }
            }
        }

        public ContentNestedData DeserializeBytes(byte[] data) => MessagePackSerializer.Deserialize<ContentNestedData>(data, _options);

        public byte[] SerializeBytes(ContentNestedData nestedData) => MessagePackSerializer.Serialize(nestedData, _options);

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
