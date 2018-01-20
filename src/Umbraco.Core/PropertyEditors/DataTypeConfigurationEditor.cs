using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Umbraco.Core.Composing;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;

namespace Umbraco.Core.PropertyEditors
{
    /// <summary>
    /// Represents a data type configuration editor.
    /// </summary>
    public class DataTypeConfigurationEditor
    {
        // a configuration editor is made up of multiple configuration fields
        // each field is identified by a key, each field has its own editor
        // the json serialization attribute is required for manifest property editors to work (fixme - datamember?)

        /// <summary>
        /// Initializes a new instance of the <see cref="DataTypeConfigurationEditor"/> class.
        /// </summary>
        public DataTypeConfigurationEditor()
        {
            Fields = DiscoverFields();
        }

        // fixme
        // do we need to support discovering fields?
        // bearing in mind that attribute.PreValueFieldType is *never* used
        // we probably can achieve the same with explicit code everywhere

        private List<DataTypeConfigurationField> DiscoverFields()
        {
            var fields = new List<DataTypeConfigurationField>();

            // discover fields that are properties marked with the field attribute
            var props = TypeHelper.CachedDiscoverableProperties(GetType()).Where(x => x.Name != "Fields");

            foreach (var prop in props)
            {
                var attribute = prop.GetCustomAttribute<DataTypeConfigurationFieldAttribute>(false);
                if (attribute == null) continue;

                DataTypeConfigurationField field;

                // if the field does not have its own type, use the base type            
                if (attribute.PreValueFieldType == null)
                {
                    field = new DataTypeConfigurationField
                    {
                        // if the key is empty then use the property name
                        Key = string.IsNullOrWhiteSpace(attribute.Key) ? prop.Name : attribute.Key,
                        Name = attribute.Name,
                        Description = attribute.Description,
                        HideLabel = attribute.HideLabel,
                        View = attribute.View.StartsWith("~/") ? IOHelper.ResolveUrl(attribute.View) : attribute.View // fixme why cant it remain unchagned?
                    };

                    fields.Add(field);
                    continue;
                }

                // if the field has its own type, instanciate it
                try
                {
                    field = (DataTypeConfigurationField) Activator.CreateInstance(attribute.PreValueFieldType);
                }
                catch (Exception ex)
                {
                    Current.Logger.Warn<PreValueEditor>(ex, $"Could not create an instance of type \"{attribute.PreValueFieldType}\".");
                    continue;
                }

                // then add it, and overwrite values if they are assigned in the attribute
                fields.Add(field);

                if (!string.IsNullOrWhiteSpace(attribute.Key))
                    field.Key = attribute.Key;

                // if the key is empty then use the property name
                if (string.IsNullOrWhiteSpace(field.Key))
                    field.Key = prop.Name;

                if (!string.IsNullOrWhiteSpace(attribute.Name))
                    field.Name = attribute.Name;

                if (!string.IsNullOrWhiteSpace(attribute.View))
                    field.View = attribute.View;

                if (!string.IsNullOrWhiteSpace(attribute.Description))
                    field.Description = attribute.Description;

                if (attribute.HideLabel)
                    field.HideLabel = attribute.HideLabel;
            }

            return fields;
        }

        /// <summary>
        /// Gets the fields.
        /// </summary>
        [JsonProperty("fields")]
        public List<DataTypeConfigurationField> Fields { get; private set; }

        /// <summary>
        /// Converts the values posted by the editor to configuration values.
        /// </summary>
        /// <param name="editorValues">The values posted by the editor.</param>
        /// <param name="configuration">The current configuration.</param>
        /// <returns></returns>
        public virtual IDictionary<string, PreValue> ConvertEditorToDb(IDictionary<string, object> editorValues, object configuration)
        {
            // fixme by default, just converting to PreValue
            //convert to a string based value to be saved in the db
            return editorValues.ToDictionary(x => x.Key, x => new PreValue(x.Value?.ToString()));
        }

        /// <summary>
        /// Converts configuration values to values for the editor.
        /// </summary>
        /// <param name="defaultConfiguration">The default configuration.</param>
        /// <param name="configuration">The configuration.</param>
        public virtual IDictionary<string, object> ConvertDbToEditor(object defaultConfiguration, object configuration)
        {}
    }
}