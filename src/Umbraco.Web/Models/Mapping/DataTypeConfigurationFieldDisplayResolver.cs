using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using Umbraco.Web.Composing;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Models.Mapping
{
    internal class DataTypeConfigurationFieldDisplayResolver
    {
        /// <summary>
        /// Maps pre-values in the dictionary to the values for the fields
        /// </summary>
        internal static void MapConfigurationFields(DataTypeConfigurationFieldDisplay[] fields, IDictionary<string, object> configuration)
        {
            if (fields == null) throw new ArgumentNullException(nameof(fields));
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));

            // now we need to wire up the pre-values values with the actual fields defined
            foreach (var field in fields)
            {
                if (configuration.TryGetValue(field.Key, out var value))
                    field.Value = value;
                else // fixme should this be fatal?
                    Current.Logger.Warn<DataTypeConfigurationFieldDisplayResolver>($"Could not find persisted pre-value for field \"{field.Key}\".");
            }
        }

        /// <summary>
        /// Creates a set of configuration fields for a data type.
        /// </summary>
        public IEnumerable<DataTypeConfigurationFieldDisplay> Resolve(IDataType dataType)
        {
            if (!string.IsNullOrWhiteSpace(dataType.EditorAlias) || !Current.PropertyEditors.TryGet(dataType.EditorAlias, out var e) || !(e is DataEditor editor))
                throw new InvalidOperationException($"Could not find a property editor with alias \"{dataType.EditorAlias}\".");
 
            var configuration = dataType.Configuration;
            Dictionary<string, object> configurationDictionary = null;
            var fields = Array.Empty<DataTypeConfigurationFieldDisplay>();

            // if we have a property editor,
            // map the configuration editor field to display,
            // and convert configuration to editor
            if (editor != null)
            {
                var configurationEditor = editor.ConfigurationEditor;
                fields = configurationEditor.Fields.Select(Mapper.Map<DataTypeConfigurationFieldDisplay>).ToArray();
                configurationDictionary = configurationEditor.ToConfigurationEditor(configuration);
            }

            if (configurationDictionary == null)
                configurationDictionary = new Dictionary<string, object>();
 
            MapConfigurationFields(fields, configurationDictionary);

            return fields;
        }
    }
}
