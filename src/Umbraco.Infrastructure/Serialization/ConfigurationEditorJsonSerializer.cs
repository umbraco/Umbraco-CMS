using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Infrastructure.Serialization;

// TODO: clean this file up (delete the old backoffice implementation when we're ready to do so)

// use this implementation for testing with the old backoffice (using Newtonsoft.Json)
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

// use this implementation for testing with the new backoffice API (using System.Text.Json)
// public class ConfigurationEditorJsonSerializer : IConfigurationEditorJsonSerializer
// {
//     private System.Text.Json.JsonSerializerOptions _jsonSerializerOptions;
//
//     public ConfigurationEditorJsonSerializer()
//     {
//         _jsonSerializerOptions = new System.Text.Json.JsonSerializerOptions
//         {
//             PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
//             // in some cases, configs aren't camel cased in the DB, so we have to resort to case insensitive
//             // property name resolving when creating configuration objects (deserializing DB configs)
//             PropertyNameCaseInsensitive = true,
//             NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString
//         };
//         _jsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
//     }
//
//     public string Serialize(object? input) => System.Text.Json.JsonSerializer.Serialize(input, _jsonSerializerOptions);
//
//     public T? Deserialize<T>(string input) => System.Text.Json.JsonSerializer.Deserialize<T>(input, _jsonSerializerOptions);
//
//     public T? DeserializeSubset<T>(string input, string key) => throw new NotSupportedException();
// }
