using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Umbraco.Core.Serialization;

namespace Umbraco.Core.PropertyEditors
{
    /// <summary>
    /// Represents a data type configuration editor.
    /// </summary>
    public class ConfigurationEditor : IConfigurationEditor
    {
        private IDictionary<string, object> _defaultConfiguration;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationEditor"/> class.
        /// </summary>
        public ConfigurationEditor()
        {
            Fields = new List<ConfigurationField>();
            _defaultConfiguration = new Dictionary<string, object>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationEditor"/> class.
        /// </summary>
        protected ConfigurationEditor(List<ConfigurationField> fields)
        {
            Fields = fields;
        }

        /// <summary>
        /// Gets the fields.
        /// </summary>
        [JsonProperty("fields")]
        public List<ConfigurationField> Fields { get; }

        /// <summary>
        /// Gets a field by its property name.
        /// </summary>
        /// <remarks>Can be used in constructors to add infos to a field that has been defined
        /// by a property marked with the <see cref="ConfigurationFieldAttribute"/>.</remarks>
        protected ConfigurationField Field(string name)
            => Fields.First(x => x.PropertyName == name);

        /// <summary>
        /// Gets the configuration as a typed object.
        /// </summary>
        public static TConfiguration ConfigurationAs<TConfiguration>(object obj)
        {
            if (obj == null) return default;
            if (obj is TConfiguration configuration) return configuration;
            throw new InvalidCastException($"Cannot cast configuration of type {obj.GetType().Name} to {typeof(TConfiguration).Name}.");
        }

        /// <summary>
        /// Converts a configuration object into a serialized database value.
        /// </summary>
        public static string ToDatabase(object configuration)
            => configuration == null ? null : JsonConvert.SerializeObject(configuration, ConfigurationJsonSettings);

        /// <inheritdoc />
        [JsonProperty("defaultConfig")]
        public virtual IDictionary<string, object> DefaultConfiguration {
            get => _defaultConfiguration;
            internal set => _defaultConfiguration = value;
        }

        /// <inheritdoc />
        public virtual object DefaultConfigurationObject => DefaultConfiguration;

        /// <inheritdoc />
        public virtual bool IsConfiguration(object obj) => obj is IDictionary<string, object>;

        /// <inheritdoc />
        public virtual object FromDatabase(string configurationJson)
            => string.IsNullOrWhiteSpace(configurationJson)
                ? new Dictionary<string, object>()
                : JsonConvert.DeserializeObject<Dictionary<string, object>>(configurationJson);

        /// <inheritdoc />
        public virtual object FromConfigurationEditor(IDictionary<string, object> editorValues, object configuration)
        {
            // by default, return the posted dictionary
            // but only keep entries that have a non-null/empty value
            // rest will fall back to default during ToConfigurationEditor()

            var keys = editorValues.Where(x =>
                    x.Value == null || x.Value is string stringValue && string.IsNullOrWhiteSpace(stringValue))
                .Select(x => x.Key).ToList();

            foreach (var key in keys) editorValues.Remove(key);

            return editorValues;
        }

        /// <inheritdoc />
        public virtual IDictionary<string, object> ToConfigurationEditor(object configuration)
        {
            // editors that do not override ToEditor/FromEditor have their configuration
            // as a dictionary of <string, object> and, by default, we merge their default
            // configuration with their current configuration

            if (configuration == null)
                configuration = new Dictionary<string, object>();

            if (!(configuration is IDictionary<string, object> c))
                throw new ArgumentException($"Expecting a {typeof(Dictionary<string,object>).Name} instance but got {configuration.GetType().Name}.", nameof(configuration));

            // clone the default configuration, and apply the current configuration values
            var d = new Dictionary<string, object>(DefaultConfiguration);
            foreach (var (key, value) in c)
                d[key] = value;
            return d;
        }

        /// <inheritdoc />
        public virtual IDictionary<string, object> ToValueEditor(object configuration)
            => ToConfigurationEditor(configuration);

        /// <summary>
        /// Gets the custom json serializer settings for configurations.
        /// </summary>
        public static JsonSerializerSettings ConfigurationJsonSettings { get; } = new JsonSerializerSettings
        {
            Formatting = Formatting.None,
            NullValueHandling = NullValueHandling.Ignore,
            ContractResolver = new ConfigurationCustomContractResolver(),
            Converters = new List<JsonConverter>(new[]{new FuzzyBooleanConverter()})
        };

        private class ConfigurationCustomContractResolver : DefaultContractResolver
        {
            protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
            {
                // base.CreateProperty deals with [JsonProperty("name")]
                var property = base.CreateProperty(member, memberSerialization);

                // override with our custom attribute, if any
                var attribute = member.GetCustomAttribute<ConfigurationFieldAttribute>();
                if (attribute != null) property.PropertyName = attribute.Key;

                // for value types,
                //  don't try to deserialize nulls (in legacy json)
                //  no impact on serialization (value cannot be null)
                if (member is PropertyInfo propertyInfo && propertyInfo.PropertyType.IsValueType)
                    property.NullValueHandling = NullValueHandling.Ignore;

                return property;
            }
        }
    }
}
