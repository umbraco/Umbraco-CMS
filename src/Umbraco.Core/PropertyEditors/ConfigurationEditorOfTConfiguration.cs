using System;
using System.Collections.Concurrent;
using System.Linq;
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
        // ReSharper disable once StaticMemberInGenericType
        private static Dictionary<string, (Type PropertyType, object Setter)> _fromObjectTypes
            = new Dictionary<string, (Type, object)>();

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
                        Description = attribute.Description,
                        HideLabel = attribute.HideLabel,
                        View = attribute.View
                    };

                    fields.Add(field);
                    continue;
                }

                // if the field has its own type, instanciate it
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
        public override object ParseConfiguration(string configuration)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(configuration)) return new TConfiguration();
                return JsonConvert.DeserializeObject<TConfiguration>(configuration);
            }
            catch (Exception e)
            {
                throw new Exception($"Failed to parse configuration \"{configuration}\" as \"{typeof(TConfiguration).Name}\" (see inner exception).", e);
            }
        }

        /// <inheritdoc />
        public sealed override object FromEditor(Dictionary<string, object> editorValue, object configuration)
        {
            return FromEditor(editorValue, (TConfiguration) configuration);
        }

        /// <summary>
        /// Converts the configuration posted by the editor.
        /// </summary>
        /// <param name="editorValue">The configuration object posted by the editor.</param>
        /// <param name="configuration">The current configuration object.</param>
        public virtual TConfiguration FromEditor(Dictionary<string, object> editorValue, TConfiguration configuration)
        {
            // note - editorValue contains a mix of Clr types (string, int...) and JToken
            // turning everything back into a JToken... might not be fastest but is simplest
            // for now

            var o = new JObject();

            foreach (var field in Fields)
            {
                // only fields - ignore json property
                // fixme should we deal with jsonProperty anyways?
                if (editorValue.TryGetValue(field.Key, out var value) && value != null)
                    o[field.PropertyName] = value is JToken jtoken ? jtoken : JToken.FromObject(value);
            }

            return o.ToObject<TConfiguration>();
        }

        /// <inheritdoc />
        public sealed override Dictionary<string, object> ToEditor(object defaultConfiguration, object configuration)
        {
            return ToEditor((TConfiguration) configuration);
        }

        /// <summary>
        /// Converts configuration values to values for the editor.
        /// </summary>
        /// <param name="defaultConfiguration">The default configuration.</param>
        /// <param name="configuration">The configuration.</param>
        public virtual Dictionary<string, object> ToEditor(TConfiguration configuration)
        {
            string FieldNamer(PropertyInfo property)
            {
                // field first
                var field = property.GetCustomAttribute<ConfigurationFieldAttribute>();
                if (field != null) return field.Key;

                // then, json property
                var jsonProperty = property.GetCustomAttribute<JsonPropertyAttribute>();
                return jsonProperty?.PropertyName ?? property.Name;
            }

            var dictionary = ObjectExtensions.ToObjectDictionary(configuration, FieldNamer);

            if (configuration is ConfigurationWithAdditionalData withAdditionalData)
                foreach (var kv in withAdditionalData.GetAdditionalValues())
                    dictionary[kv.Key] = kv.Value;

            return dictionary;
        }

        /// <summary>
        /// Converts a dictionary into an object.
        /// </summary>
        /// <typeparam name="T">The type of the object.</typeparam>
        /// <param name="source">The source dictionary.</param>
        /// <returns>The object corresponding to the dictionary.</returns>
        protected T FromObjectDictionary<T>(Dictionary<string, object> source) // fixme KILL - NOT USED ANYMORE
            where T : new()
        {
            // this needs to be here (and not in ObjectExtensions) because it is based on fields

            var t = typeof(T);

            if (_fromObjectTypes == null)
            {
                var p = t.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
                var fromObjectTypes = new Dictionary<string, (Type, object)>();

                foreach (var field in Fields)
                {
                    var fp = p.FirstOrDefault(x => x.Name == field.PropertyName);
                    if (fp == null) continue;

                    fromObjectTypes[field.Key] = (fp.PropertyType, ReflectionUtilities.EmitPropertySetter<T, object>(fp));
                }

                _fromObjectTypes = fromObjectTypes;
            }

            var obj = new T();

            foreach (var field in Fields)
            {
                if (!_fromObjectTypes.TryGetValue(field.Key, out var ps)) continue;
                if (!source.TryGetValue(field.Key, out var value)) continue;

                if (ps.PropertyType.IsValueType)
                {
                    if (value == null)
                        throw new InvalidCastException($"Cannot cast null value to {ps.PropertyType.Name}.");
                }
                else
                {
                    // ReSharper disable once UseMethodIsInstanceOfType
                    if (!ps.PropertyType.IsAssignableFrom(value.GetType()))
                        throw new InvalidCastException($"Cannot cast value of type {value.GetType()} to {ps.PropertyType.Name}.");
                }

                ((Action<T, object>) ps.Setter)(obj, value);
            }

            return obj;
        }
    }
}