using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Umbraco.Core.PropertyEditors
{
    /// <summary>
    /// Represents a data type configuration editor.
    /// </summary>
    public class ConfigurationEditor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationEditor"/> class.
        /// </summary>
        public ConfigurationEditor()
        {
            Fields = new List<ConfigurationField>();
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
        /// Gets a field by property name.
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
        /// Gets the default configuration.
        /// </summary>
        /// <remarks>
        /// <para>The default configuration is used to initialize new datatypes.</para>
        /// </remarks>
        [JsonProperty("defaultConfig")]
        public virtual IDictionary<string, object> DefaultConfiguration => new Dictionary<string, object>();

        /// <summary>
        /// Determines whether a configuration object is of the type expected by the configuration editor.
        /// </summary>
        public virtual bool IsConfiguration(object obj)
            => obj is IDictionary<string, object>;

        // notes
        // ToConfigurationEditor returns a dictionary, and FromConfigurationEditor accepts a dictionary.
        // this is due to the way our front-end editors work, see DataTypeController.PostSave
        // and DataTypeConfigurationFieldDisplayResolver - we are not going to change it now.

        /// <summary>
        /// Converts the serialized database value into the actual configuration object.
        /// </summary>
        /// <remarks>Converting the configuration object to the serialized database value is
        /// achieved by simply serializing the configuration.</remarks>
        public virtual object FromDatabase(string configurationJson)
            => string.IsNullOrWhiteSpace(configurationJson)
                ? new Dictionary<string, object>()
                : JsonConvert.DeserializeObject<Dictionary<string, object>>(configurationJson);

        /// <summary>
        /// Converts the values posted by the configuration editor into the actual configuration object.
        /// </summary>
        /// <param name="editorValues">The values posted by the configuration editor.</param>
        /// <param name="configuration">The current configuration object.</param>
        public virtual object FromConfigurationEditor(Dictionary<string, object> editorValues, object configuration)
        {
            // by default, return the posted dictionary
            // but only keep entries that have a non-null/empty value
            // rest will fall back to default during ToConfigurationEditor()

            var keys = editorValues.Where(x => x.Value == null || x.Value is string stringValue && string.IsNullOrWhiteSpace(stringValue)).Select(x => x.Key);
            foreach (var key in keys) editorValues.Remove(key);

            return editorValues;
        }

        /// <summary>
        /// Converts the configuration object to values for the configuration editor.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        public virtual Dictionary<string, object> ToConfigurationEditor(object configuration)
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
            foreach ((var key, var value) in c)
                d[key] = value;
            return d;
        }

        /// <summary>
        /// Converts the configuration object to values for the value editror.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        public virtual Dictionary<string, object> ToValueEditor(object configuration)
            => ToConfigurationEditor(configuration);
    }
}