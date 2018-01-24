using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Core.Logging;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Serialization;

namespace Umbraco.Core.Manifest
{
    /// <summary>
    /// Implements a json read converter for <see cref="PropertyEditor"/>.
    /// </summary>
    internal class PropertyEditorConverter : JsonReadConverter<PropertyEditor>
    {
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyEditorConverter"/> class.
        /// </summary>
        public PropertyEditorConverter(ILogger logger)
        {
            _logger = logger;
        }

        /// <inheritdoc />
        protected override PropertyEditor Create(Type objectType, JObject jObject)
        {
            return new PropertyEditor(_logger);
        }

        /// <inheritdoc />
        protected override void Deserialize(JObject jobject, PropertyEditor target, JsonSerializer serializer)
        {
            if (jobject["editor"] != null)
            {
                // the deserializer will first try to get the property, and that would throw since
                // the editor would try to create a new value editor, so we have to set a
                // value editor by ourselves, which will then be populated by the deserializer.
                target.ValueEditor = new ValueEditor();

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

            // see note about validators, above - same applies to field validators
            if (jobject["prevalues"]?["fields"] is JArray jarray)
            {
                foreach (var field in jarray)
                {
                    // see note above, for editor
                    if (field["validation"] is JObject validation)
                        field["validation"] = RewriteValidators(validation);
                }
            }

            base.Deserialize(jobject, target, serializer);
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
