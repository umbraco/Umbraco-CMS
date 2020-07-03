using MessagePack;
using System;

namespace Umbraco.Web.PublishedCache.NuCache.DataSource
{
    internal class MsgPackContentNestedDataSerializer : IContentNestedDataSerializer
    {
        private MessagePackSerializerOptions _options;

        public MsgPackContentNestedDataSerializer()
        {
            _options = MessagePack.Resolvers.ContractlessStandardResolver.Options.WithCompression(MessagePackCompression.Lz4BlockArray);
        }

        public string ToJson(string serialized)
        {
            var bin = Convert.FromBase64String(serialized);
            var json = MessagePackSerializer.ConvertToJson(bin, _options);
            return json;
        }

        // TODO: Instead of returning base64 it would be more ideal to avoid that translation entirely and just store/retrieve raw bytes

        // TODO: We need to write tests to serialize/deserialize between either of these serializers to ensure we end up with the same object
        // i think this one is a bit quirky so far :)

        public ContentNestedData Deserialize(string data)
        {
            var bin = Convert.FromBase64String(data);
            var obj = MessagePackSerializer.Deserialize<ContentNestedData>(bin, _options);
            return obj;
        }

        public string Serialize(ContentNestedData nestedData)
        {            
            var bin = MessagePackSerializer.Serialize(
              nestedData,
              _options);
            return Convert.ToBase64String(bin);
        }
    }
}
