﻿using K4os.Compression.LZ4;
using MessagePack;
using MessagePack.Resolvers;
using System;
using System.Linq;
using System.Text;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PublishedCache.NuCache.DataSource
{

    /// <summary>
    /// Serializes/Deserializes <see cref="ContentCacheDataModel"/> document to the SQL Database as bytes using MessagePack
    /// </summary>
    public class MsgPackContentNestedDataSerializer : IContentCacheDataSerializer
    {
        private readonly MessagePackSerializerOptions _options;
        private readonly IPropertyCacheCompression _propertyOptions;

        public MsgPackContentNestedDataSerializer(IPropertyCacheCompression propertyOptions)
        {
            _propertyOptions = propertyOptions ?? throw new ArgumentNullException(nameof(propertyOptions));

            var defaultOptions = ContractlessStandardResolver.Options;
            var resolver = CompositeResolver.Create(

                // TODO: We want to be able to intern the strings for aliases when deserializing like we do for Newtonsoft but I'm unsure exactly how
                // to do that but it would seem to be with a custom message pack resolver but I haven't quite figured out based on the docs how
                // to do that since that is part of the int key -> string mapping operation, might have to see the source code to figure that one out.
                // There are docs here on how to build one of these: https://github.com/neuecc/MessagePack-CSharp/blob/master/README.md#low-level-api-imessagepackformattert
                // and there are a couple examples if you search on google for them but this will need to be a separate project.
                // NOTE: resolver custom types first
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

        public ContentCacheDataModel Deserialize(IReadOnlyContentBase content, string stringData, byte[] byteData)
        {
            if (byteData != null)
            {
                var cacheModel = MessagePackSerializer.Deserialize<ContentCacheDataModel>(byteData, _options);
                Expand(content, cacheModel);
                return cacheModel;
            }
            else if (stringData != null)
            {
                // NOTE: We don't really support strings but it's possible if manually used (i.e. tests)
                var bin = Convert.FromBase64String(stringData);
                var cacheModel = MessagePackSerializer.Deserialize<ContentCacheDataModel>(bin, _options);
                Expand(content, cacheModel);
                return cacheModel;
            }
            else
            {
                return null;
            }
        }

        public ContentCacheDataSerializationResult Serialize(IReadOnlyContentBase content, ContentCacheDataModel model)
        {
            Compress(content, model);
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
        private void Compress(IReadOnlyContentBase content, ContentCacheDataModel model)
        {
            foreach(var propertyAliasToData in model.PropertyData)
            {
                if (_propertyOptions.IsCompressed(content, propertyAliasToData.Key))
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
        private void Expand(IReadOnlyContentBase content, ContentCacheDataModel nestedData)
        {
            foreach (var propertyAliasToData in nestedData.PropertyData)
            {
                if (_propertyOptions.IsCompressed(content, propertyAliasToData.Key))
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
    }
}
