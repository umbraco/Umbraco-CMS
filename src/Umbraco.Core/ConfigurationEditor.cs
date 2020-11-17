using System;
using System.Collections.Generic;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Core
{
    /// <summary>
    /// Represents a data type configuration editor.
    /// </summary>
    /// TODO: Migrate the ConfigurationEditor from Infrastructure to Umbraco.Core.PropertyEditors - having a draft of the class in Core, so we can migrate more Models
    public class ConfigurationEditor : IConfigurationEditor
    {
        public List<ConfigurationField> Fields { get; }
        public IDictionary<string, object> DefaultConfiguration { get; }
        public object DefaultConfigurationObject { get; }
        public bool IsConfiguration(object obj)
        {
            throw new NotImplementedException();
        }

        public object FromDatabase(string configurationJson)
        {
            throw new NotImplementedException();
        }

        public object FromConfigurationEditor(IDictionary<string, object> editorValues, object configuration)
        {
            throw new NotImplementedException();
        }

        public IDictionary<string, object> ToConfigurationEditor(object configuration)
        {
            throw new NotImplementedException();
        }

        public IDictionary<string, object> ToValueEditor(object configuration)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the configuration as a typed object.
        /// </summary>
        public static TConfiguration ConfigurationAs<TConfiguration>(object obj)
        {
            if (obj == null) return default;
            if (obj is TConfiguration configuration) return configuration;
            throw new InvalidCastException($"Cannot cast configuration of type {obj.GetType().Name} to {typeof(TConfiguration).Name}.");
        }
    }
}
