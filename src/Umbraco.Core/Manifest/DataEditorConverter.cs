using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Core.Logging;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Serialization;

namespace Umbraco.Core.Manifest
{
    /// <summary>
    /// Provides a json read converter for <see cref="IDataEditor"/> in manifests.
    /// </summary>
    internal class DataEditorConverter : JsonReadConverter<IDataEditor>
    {
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataEditorConverter"/> class.
        /// </summary>
        public DataEditorConverter(ILogger logger)
        {
            _logger = logger;
        }

        /// <inheritdoc />
        protected override IDataEditor Create(Type objectType, JObject jobject)
        {
            // in PackageManifest, property editors are IConfiguredDataEditor[] whereas
            // parameter editors are IDataEditor[] - both will end up here because we handle
            // IDataEditor and IConfiguredDataEditor implements it, but we can check the
            // type to figure out what to create

            if (objectType == typeof(IConfiguredDataEditor))
            {
                // property editor
                var type = EditorType.PropertyValue;
                if (jobject["isParameterEditor"] is JToken jToken && jToken.Value<bool>())
                    type &= EditorType.MacroParameter;
                return new ConfiguredDataEditor(_logger, type);
            }
            else
            {
                // parameter editor
                return new DataEditor(EditorType.MacroParameter);
            }
        }

        /// <inheritdoc />
        protected override void Deserialize(JObject jobject, IDataEditor target, JsonSerializer serializer)
        {
            // see Create above, target is either DataEditor (parameter) or ConfiguredDataEditor (property)

            if (target is ConfiguredDataEditor configuredEditor)
            {
                // property editor
                PrepareForPropertyEditor(jobject, configuredEditor);
            }
            else if (target is DataEditor editor)
            {
                // parameter editor
                PrepareForParameterEditor(jobject, editor);
            }
            else throw new Exception("panic.");

            base.Deserialize(jobject, target, serializer);
        }

        private static void PrepareForPropertyEditor(JObject jobject, ConfiguredDataEditor target)
        {
            if (jobject["editor"] != null)
            {
                // explicitely assign a value editor of type ValueEditor
                // (else the deserializer will try to read it before setting it)
                // (and besides it's an interface)
                target.ValueEditor = new DataValueEditor();

                // in the manifest, validators are a simple dictionary eg
                // {
                //   required: true,
                //   regex: '\\d*'
                // }
                // and we need to turn this into a list of IPropertyValidator
                // so, rewrite the json structure accordingly
                if (jobject["editor"]["validation"] is JObject validation)
                    jobject["editor"]["validation"] = RewriteValidators(validation);
            }

            if (jobject["prevalues"] is JObject prevalues)
            {
                // explicitely assign a configuration editor of type ConfigurationEditor
                // (else the deserializer will try to read it before setting it)
                // (and besides it's an interface)
                target.ConfigurationEditor = new ConfigurationEditor();

                // see note about validators, above - same applies to field validators
                if (jobject["prevalues"]?["fields"] is JArray jarray)
                {
                    foreach (var field in jarray)
                    {
                        if (field["validation"] is JObject validation)
                            field["validation"] = RewriteValidators(validation);
                    }
                }

                // in the manifest, default configuration is at editor level
                // move it down to configuration editor level so it can be deserialized properly
                if (jobject["defaultConfig"] is JObject defaultConfig)
                {
                    prevalues["defaultConfig"] = defaultConfig;
                    jobject.Remove("defaultConfig");
                }
            }
        }

        private static void PrepareForParameterEditor(JObject jobject, DataEditor target)
        {
            // in a manifest, a parameter editor looks like:
            //
            // {
            //   "alias": "...",
            //   "name": "...",
            //   "view": "...",
            //   "config": { "key1": "value1", "key2": "value2" ... }
            // }
            //
            // the view is at top level, but should be down one level to be propertly
            // deserialized as a ParameterValueEditor property -> need to move it

            if (jobject.Property("view") != null)
            {
                // explicitely assign a value editor of type ParameterValueEditor
                target.ValueEditor = new DataValueEditor();

                // move the 'view' property
                jobject["editor"] = new JObject { ["view"] = jobject["view"] };
                jobject.Property("view").Remove();
            }
        }

        private static JArray RewriteValidators(JObject validation)
        {
            var jarray = new JArray();

            foreach (var v in validation)
            {
                var key = v.Key;
                var val = v.Value?.Type == JTokenType.Boolean ? string.Empty : v.Value;
                var jo = new JObject { { "type", key }, { "config", val } };
                jarray.Add(jo);
            }

            return jarray;
        }
    }
}
