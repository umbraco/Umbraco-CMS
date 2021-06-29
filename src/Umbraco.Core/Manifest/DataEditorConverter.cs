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
        protected override IDataEditor Create(Type objectType, string path, JObject jobject)
        {
            // in PackageManifest, property editors are IConfiguredDataEditor[] whereas
            // parameter editors are IDataEditor[] - both will end up here because we handle
            // IDataEditor and IConfiguredDataEditor implements it, but we can check the
            // type to figure out what to create

            var type = EditorType.PropertyValue;

            var isPropertyEditor = path.StartsWith("propertyEditors[");

            if (isPropertyEditor)
            {
                // property editor
                jobject["isPropertyEditor"] = JToken.FromObject(true);
                if (jobject["isParameterEditor"] is JToken jToken && jToken.Value<bool>())
                    type |= EditorType.MacroParameter;
            }
            else
            {
                // parameter editor
                type = EditorType.MacroParameter;
            }

            return new DataEditor(_logger, type);
        }

        /// <inheritdoc />
        protected override void Deserialize(JObject jobject, IDataEditor target, JsonSerializer serializer)
        {
            // see Create above, target is either DataEditor (parameter) or ConfiguredDataEditor (property)

            if (!(target is DataEditor dataEditor))
                throw new Exception("panic.");

            if (jobject["isPropertyEditor"] is JToken jtoken && jtoken.Value<bool>())
                PrepareForPropertyEditor(jobject, dataEditor);
            else
                PrepareForParameterEditor(jobject, dataEditor);

            base.Deserialize(jobject, target, serializer);
        }

        private static void PrepareForPropertyEditor(JObject jobject, DataEditor target)
        {
            if (jobject["editor"] == null)
                throw new InvalidOperationException("Missing 'editor' value.");

            // explicitly assign a value editor of type ValueEditor
            // (else the deserializer will try to read it before setting it)
            // (and besides it's an interface)
            target.ExplicitValueEditor = new DataValueEditor();

            // in the manifest, validators are a simple dictionary eg
            // {
            //   required: true,
            //   regex: '\\d*'
            // }
            // and we need to turn this into a list of IPropertyValidator
            // so, rewrite the json structure accordingly
            if (jobject["editor"]["validation"] is JObject validation)
                jobject["editor"]["validation"] = RewriteValidators(validation);

            var prevalues = jobject["prevalues"] as JObject;
            var defaultConfig = jobject["defaultConfig"] as JObject;
            if (prevalues != null || defaultConfig != null)
            {
                // explicitly assign a configuration editor of type ConfigurationEditor
                // (else the deserializer will try to read it before setting it)
                // (and besides it's an interface)
                target.ExplicitConfigurationEditor = new ConfigurationEditor();

                var config = new JObject();
                if (prevalues != null)
                {
                    config = prevalues;
                    // see note about validators, above - same applies to field validators
                    if (config["fields"] is JArray jarray)
                    {
                        foreach (var field in jarray)
                        {
                            if (field["validation"] is JObject fvalidation)
                                field["validation"] = RewriteValidators(fvalidation);
                        }
                    }
                }

                // in the manifest, default configuration is at editor level
                // move it down to configuration editor level so it can be deserialized properly
                if (defaultConfig != null)
                {
                    config["defaultConfig"] = defaultConfig;
                    jobject.Remove("defaultConfig");
                }

                // in the manifest, configuration is named 'prevalues', rename
                // it is important to do this LAST
                jobject["config"] = config;
                jobject.Remove("prevalues");
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
            // the view is at top level, but should be down one level to be properly
            // deserialized as a ParameterValueEditor property -> need to move it

            if (jobject.Property("view") != null)
            {
                // explicitly assign a value editor of type ParameterValueEditor
                target.ExplicitValueEditor = new DataValueEditor();

                // move the 'view' property
                jobject["editor"] = new JObject { ["view"] = jobject["view"] };
                jobject.Property("view").Remove();
            }

            // in the manifest, default configuration is named 'config', rename
            if (jobject["config"] is JObject config)
            {
                jobject["defaultConfig"] = config;
                jobject.Remove("config");
            }
        }

        private static JArray RewriteValidators(JObject validation)
        {
            var jarray = new JArray();

            foreach (var v in validation)
            {
                var key = v.Key;
                var val = v.Value;
                var jo = new JObject { { "type", key }, { "configuration", val } };
                jarray.Add(jo);
            }

            return jarray;
        }
    }
}
