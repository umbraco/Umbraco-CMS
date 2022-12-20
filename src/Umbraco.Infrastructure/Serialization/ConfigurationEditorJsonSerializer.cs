// TODO: clean this file up (delete the old backoffice implementation when we're ready to do so)

// ############################################################################################
// use this implementation for testing with the old backoffice (using Newtonsoft.Json)
// ############################################################################################

using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Infrastructure.Serialization;

public class ConfigurationEditorJsonSerializer : JsonNetSerializer, IConfigurationEditorJsonSerializer
{
    public ConfigurationEditorJsonSerializer()
    {
        JsonSerializerSettings.Converters.Add(new FuzzyBooleanConverter());
        JsonSerializerSettings.ContractResolver = new ConfigurationCustomContractResolver();
        JsonSerializerSettings.Formatting = Formatting.None;
        JsonSerializerSettings.NullValueHandling = NullValueHandling.Ignore;
    }

    private class ConfigurationCustomContractResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            // base.CreateProperty deals with [JsonProperty("name")]
            JsonProperty property = base.CreateProperty(member, memberSerialization);

            // override with our custom attribute, if any
            ConfigurationFieldAttribute? attribute = member.GetCustomAttribute<ConfigurationFieldAttribute>();
            if (attribute != null)
            {
                property.PropertyName = attribute.Key;
            }

            // for value types,
            //  don't try to deserialize nulls (in legacy json)
            //  no impact on serialization (value cannot be null)
            if (member is PropertyInfo propertyInfo && propertyInfo.PropertyType.IsValueType)
            {
                property.NullValueHandling = NullValueHandling.Ignore;
            }

            return property;
        }
    }
}

// ############################################################################################
// use this implementation for testing with the new backoffice API (using System.Text.Json)
// ############################################################################################

// using System.Text.Json;
// using System.Text.Json.Nodes;
// using Umbraco.Cms.Core.Serialization;
//
// namespace Umbraco.Cms.Infrastructure.Serialization;
//
// public class ConfigurationEditorJsonSerializer : IConfigurationEditorJsonSerializer
// {
//     private JsonSerializerOptions _jsonSerializerOptions;
//
//     public ConfigurationEditorJsonSerializer()
//     {
//         _jsonSerializerOptions = new JsonSerializerOptions
//         {
//             PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
//             // in some cases, configs aren't camel cased in the DB, so we have to resort to case insensitive
//             // property name resolving when creating configuration objects (deserializing DB configs)
//             PropertyNameCaseInsensitive = true,
//             NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString
//         };
//         _jsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
//         _jsonSerializerOptions.Converters.Add(new JsonObjectConverter());
//     }
//
//     public string Serialize(object? input) => JsonSerializer.Serialize(input, _jsonSerializerOptions);
//
//     public T? Deserialize<T>(string input) => JsonSerializer.Deserialize<T>(input, _jsonSerializerOptions);
//
//     public T? DeserializeSubset<T>(string input, string key) => throw new NotSupportedException();
//
//     // TODO: reuse the JsonObjectConverter implementation from management API
//     public class JsonObjectConverter : System.Text.Json.Serialization.JsonConverter<object>
//     {
//         public override object Read(
//             ref Utf8JsonReader reader,
//             Type typeToConvert,
//             JsonSerializerOptions options) =>
//             ParseObject(ref reader);
//
//         public override void Write(
//             Utf8JsonWriter writer,
//             object objectToWrite,
//             JsonSerializerOptions options)
//         {
//             if (objectToWrite is null)
//             {
//                 return;
//             }
//
//             // If an object is equals "new object()", Json.Serialize would recurse forever and cause a stack overflow
//             // We have no good way of checking if its an empty object
//             // which is why we try to check if the object has any properties, and thus will be empty.
//             if (objectToWrite.GetType().Name is "Object" && !objectToWrite.GetType().GetProperties().Any())
//             {
//                 writer.WriteStartObject();
//                 writer.WriteEndObject();
//             }
//             else
//             {
//                 JsonSerializer.Serialize(writer, objectToWrite, objectToWrite.GetType(), options);
//             }
//         }
//
//         private object ParseObject(ref Utf8JsonReader reader)
//         {
//             if (reader.TokenType == JsonTokenType.StartArray)
//             {
//                 var items = new List<object>();
//                 while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
//                 {
//                     items.Add(ParseObject(ref reader));
//                 }
//
//                 return items.ToArray();
//             }
//
//             if (reader.TokenType == JsonTokenType.StartObject)
//             {
//                 var jsonNode = JsonNode.Parse(ref reader);
//                 if (jsonNode is JsonObject jsonObject)
//                 {
//                     return jsonObject;
//                 }
//             }
//
//             return reader.TokenType switch
//             {
//                 JsonTokenType.True => true,
//                 JsonTokenType.False => false,
//                 JsonTokenType.Number when reader.TryGetInt32(out int i) => i,
//                 JsonTokenType.Number when reader.TryGetInt64(out long l) => l,
//                 JsonTokenType.Number => reader.GetDouble(),
//                 JsonTokenType.String when reader.TryGetDateTime(out DateTime datetime) => datetime,
//                 JsonTokenType.String => reader.GetString()!,
//                 _ => JsonDocument.ParseValue(ref reader).RootElement.Clone()
//             };
//         }
//     }
// }
