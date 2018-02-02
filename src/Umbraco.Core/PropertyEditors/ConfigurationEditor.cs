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

        protected void AddField(ConfigurationField field)
        {
            var existing = Fields.FirstOrDefault(x => x.Key == field.Key);
            if (existing != null)
                Fields[Fields.IndexOf(existing)] = field;
            else
                Fields.Add(field);
        }

        /// <summary>
        /// Parses the configuration.
        /// </summary>
        /// <remarks>Used to create the actual configuration dictionary from the database value.</remarks>
        public virtual object ParseConfiguration(string configurationJson)
            => string.IsNullOrWhiteSpace(configurationJson)
                ? new Dictionary<string, object>()
                : JsonConvert.DeserializeObject<Dictionary<string, object>>(configurationJson);

        /// <summary>
        /// Gets the configuration as a typed object.
        /// </summary>
        public static TConfiguration ConfigurationAs<TConfiguration>(object obj)
        {
            if (obj == null) return default;
            if (obj is TConfiguration configuration) return configuration;
            throw new InvalidCastException($"Cannot cast configuration of type {obj.GetType().Name} to {typeof(TConfiguration).Name}.");
        }

        // notes
        // ToEditor returns a dictionary, and FromEditor accepts a dictionary.
        // this is due to the way our front-end editors work, see DataTypeController.PostSave
        // and DataTypeConfigurationFieldDisplayResolver - we are not going to change it now.

        /// <summary>
        /// Converts the configuration posted by the editor.
        /// </summary>
        /// <param name="editorValue">The configuration object posted by the editor.</param>
        /// <param name="configuration">The current configuration object.</param>
        public virtual object FromEditor(Dictionary<string, object> editorValue, object configuration)
        {
            // by default, return the posted dictionary
            return editorValue;
        }

        /// <summary>
        /// Converts configuration values to values for the editor.
        /// </summary>
        /// <param name="defaultConfiguration">The default configuration.</param>
        /// <param name="configuration">The configuration.</param>
        public virtual Dictionary<string, object> ToEditor(object defaultConfiguration, object configuration)
        {
            // editors that do not override ToEditor/FromEditor have their configuration
            // as a dictionary of <string, object> and, by default, we merge their default
            // configuration with their current configuration

            // make sure we have dictionaries
            if (defaultConfiguration == null)
                defaultConfiguration = new Dictionary<string, object>();

            if (!(defaultConfiguration is IDictionary<string, object> d))
                throw new ArgumentException($"Expecting a {typeof(Dictionary<string,object>).Name} instance but got {defaultConfiguration.GetType().Name}.", nameof(defaultConfiguration));

            if (configuration == null)
                configuration = new Dictionary<string, object>();

            if (!(configuration is IDictionary<string, object> c))
                throw new ArgumentException($"Expecting a {typeof(Dictionary<string,object>).Name} instance but got {configuration.GetType().Name}.", nameof(configuration));

            // clone the default configuration, and apply the current configuration values
            var dc = new Dictionary<string, object>(d);
            foreach ((var key, var value) in c)
                dc[key] = value;
            return dc;
        }
    }
}