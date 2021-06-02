using System;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Core.Composing;

namespace Umbraco.Core.PropertyEditors
{
    /// <summary>
    /// Represents a data type configuration editor with a typed configuration.
    /// </summary>
    public abstract class ConfigurationEditor<TConfiguration> : ConfigurationEditor
        where TConfiguration : new()
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationEditor{TConfiguration}"/> class.
        /// </summary>
        protected ConfigurationEditor()
            : base(DiscoverFields())
        { }

        /// <summary>
        /// Discovers fields from configuration properties marked with the field attribute.
        /// </summary>
        private static List<ConfigurationField> DiscoverFields()
        {
            var fields = new List<ConfigurationField>();
            var properties = TypeHelper.CachedDiscoverableProperties(typeof(TConfiguration));

            foreach (var property in properties)
            {
                var attribute = property.GetCustomAttribute<ConfigurationFieldAttribute>(false);
                if (attribute == null) continue;

                ConfigurationField field;

                // if the field does not have its own type, use the base type
                if (attribute.Type == null)
                {
                    field = new ConfigurationField
                    {
                        // if the key is empty then use the property name
                        Key = string.IsNullOrWhiteSpace(attribute.Key) ? property.Name : attribute.Key,
                        Name = attribute.Name,
                        PropertyName = property.Name,
                        PropertyType = property.PropertyType,
                        Description = attribute.Description,
                        HideLabel = attribute.HideLabel,
                        View = attribute.View
                    };

                    fields.Add(field);
                    continue;
                }

                // if the field has its own type, instantiate it
                try
                {
                    field = (ConfigurationField) Activator.CreateInstance(attribute.Type);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Failed to create an instance of type \"{attribute.Type}\" for property \"{property.Name}\" of configuration \"{typeof(TConfiguration).Name}\" (see inner exception).", ex);
                }

                // then add it, and overwrite values if they are assigned in the attribute
                fields.Add(field);

                field.PropertyName = property.Name;
                field.PropertyType = property.PropertyType;

                if (!string.IsNullOrWhiteSpace(attribute.Key))
                    field.Key = attribute.Key;

                // if the key is still empty then use the property name
                if (string.IsNullOrWhiteSpace(field.Key))
                    field.Key = property.Name;

                if (!string.IsNullOrWhiteSpace(attribute.Name))
                    field.Name = attribute.Name;

                if (!string.IsNullOrWhiteSpace(attribute.View))
                    field.View = attribute.View;

                if (!string.IsNullOrWhiteSpace(attribute.Description))
                    field.Description = attribute.Description;

                if (attribute.HideLabelSettable.HasValue)
                    field.HideLabel = attribute.HideLabel;
            }

            return fields;
        }

        /// <inheritdoc />
        public override IDictionary<string, object> DefaultConfiguration => ToConfigurationEditor(DefaultConfigurationObject);

        /// <inheritdoc />
        public override object DefaultConfigurationObject => new TConfiguration();

        /// <inheritdoc />
        public override bool IsConfiguration(object obj)
            => obj is TConfiguration;

        /// <inheritdoc />
        public override object FromDatabase(string configuration)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(configuration)) return new TConfiguration();
                return JsonConvert.DeserializeObject<TConfiguration>(configuration, ConfigurationJsonSettings);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException($"Failed to parse configuration \"{configuration}\" as \"{typeof(TConfiguration).Name}\" (see inner exception).", e);
            }
        }

        /// <inheritdoc />
        public sealed override object FromConfigurationEditor(IDictionary<string, object> editorValues, object configuration)
        {
            return FromConfigurationEditor(editorValues, (TConfiguration) configuration);
        }

        /// <summary>
        /// Converts the configuration posted by the editor.
        /// </summary>
        /// <param name="editorValues">The configuration object posted by the editor.</param>
        /// <param name="configuration">The current configuration object.</param>
        public virtual TConfiguration FromConfigurationEditor(IDictionary<string, object> editorValues, TConfiguration configuration)
        {
            // note - editorValue contains a mix of CLR types (string, int...) and JToken
            // turning everything back into a JToken... might not be fastest but is simplest
            // for now

            var o = new JObject();

            foreach (var field in Fields)
            {
                // field only, JsonPropertyAttribute is ignored here
                // only keep fields that have a non-null/empty value
                // rest will fall back to default during ToObject()
                if (editorValues.TryGetValue(field.Key, out var value) && value != null && (!(value is string stringValue) || !string.IsNullOrWhiteSpace(stringValue)))
                {
                    if (value is JToken jtoken)
                    {
                        //if it's a jtoken then set it
                        o[field.PropertyName] = jtoken;
                    }
                    else if (field.PropertyType == typeof(bool) && value is string sBool)
                    {
                        //if it's a boolean property type but a string is found, try to do a conversion
                        var converted = sBool.TryConvertTo<bool>();
                        if (converted)
                            o[field.PropertyName] = converted.Result;
                    }
                    else
                    {
                        //default behavior
                        o[field.PropertyName] = JToken.FromObject(value);
                    }
                }
            }

            return o.ToObject<TConfiguration>();
        }

        /// <inheritdoc />
        public sealed override IDictionary<string, object> ToConfigurationEditor(object configuration)
        {
            return ToConfigurationEditor((TConfiguration) configuration);
        }

        /// <summary>
        /// Converts configuration values to values for the editor.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        public virtual Dictionary<string, object> ToConfigurationEditor(TConfiguration configuration)
        {
            string FieldNamer(PropertyInfo property)
            {
                // try the field
                var field = property.GetCustomAttribute<ConfigurationFieldAttribute>();
                if (field != null) return field.Key;

                // but the property may not be a field just an extra thing
                var json = property.GetCustomAttribute<JsonPropertyAttribute>();
                return json?.PropertyName ?? property.Name;
            }

            return ObjectExtensions.ToObjectDictionary(configuration, FieldNamer);
        }
    }
}
