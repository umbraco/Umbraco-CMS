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
        Settings.Converters.Add(new FuzzyBooleanConverter());
        Settings.ContractResolver = new ConfigurationCustomContractResolver();
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
