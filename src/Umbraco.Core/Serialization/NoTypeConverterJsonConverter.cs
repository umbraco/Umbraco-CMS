using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Umbraco.Core.Serialization
{
    /// <summary>
    /// This is required if we want to force JSON.Net to not use .Net TypeConverters during serialization/deserialization
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <remarks>
    /// In some cases this is required if your model has an explicit type converter, see: http://stackoverflow.com/a/31328131/694494
    ///
    /// NOTE: I was going to use this for the ImageCropDataSetConverter to convert to String, which would have worked by putting this attribute:
    /// [JsonConverter(typeof(NoTypeConverterJsonConverter{ImageCropDataSet}))] on top of the ImageCropDataSet class, however it turns out we
    /// don't require this because to convert to string, we just override ToString().
    /// I'll leave this class here for the future though.
    /// </remarks>
    internal class NoTypeConverterJsonConverter<T> : JsonConverter
    {
        static readonly IContractResolver resolver = new NoTypeConverterContractResolver();
        private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings { ContractResolver = resolver };

        private class NoTypeConverterContractResolver : DefaultContractResolver
        {
            protected override JsonContract CreateContract(Type objectType)
            {
                if (typeof(T).IsAssignableFrom(objectType))
                {
                    var contract = this.CreateObjectContract(objectType);
                    contract.Converter = null; // Also null out the converter to prevent infinite recursion.
                    return contract;
                }
                return base.CreateContract(objectType);
            }
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(T).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return JsonSerializer.CreateDefault(JsonSerializerSettings).Deserialize(reader, objectType);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            JsonSerializer.CreateDefault(JsonSerializerSettings).Serialize(writer, value);
        }
    }
}
