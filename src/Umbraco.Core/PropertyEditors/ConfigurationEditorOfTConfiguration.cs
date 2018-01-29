using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;
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
                if (string.IsNullOrWhiteSpace(configuration)) return default;
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
            return ToObject<TConfiguration>(editorValue);
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
            var dictionary = ObjectExtensions.ToObjectDictionary(configuration);

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
        protected T ToObject<T>(Dictionary<string, object> source)
            where T : new()
        {
            // fixme cache! see also ToObject in ObjectExtensions
            // this is probably very bad, must REAFACTOR! the property setter of course cannot work like this!
            //var properties = TypeHelper.CachedDiscoverableProperties(typeof(T))
            //    .ToDictionary(x => x.Name, x => (Type: x.PropertyType, Set: ReflectionUtilities.EmitPropertySetter<object, object>(x)));
            var properties = TypeHelper.CachedDiscoverableProperties(typeof(T))
                .ToDictionary(x => x.Name, x => (Type: x.PropertyType, Infos: x));

            var obj = new T();

            foreach (var field in Fields)
            {
                if (!properties.TryGetValue(field.PropertyName, out var property)) continue;
                if (!source.TryGetValue(field.Key, out var value)) continue;
                // fixme if value is null? is this what we want?
                if (!value.GetType().IsInstanceOfType(property.Type))
                    throw new Exception();
                //property.Set(obj, value);
                property.Infos.SetValue(obj, value);
            }

            return obj;
        }
    }
}