using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Serialization;

namespace Umbraco.Core.Manifest
{
    /// <summary>
    /// Implements a json read converter for <see cref="ParameterEditor"/>.
    /// </summary>
    internal class ParameterEditorConverter : JsonReadConverter<ParameterEditor>
    {
        /// <inheritdoc />
        protected override ParameterEditor Create(Type objectType, JObject jObject)
        {
            return new ParameterEditor();

        }
        /// <inheritdoc />
        protected override void Deserialize(JObject jobject, ParameterEditor target, JsonSerializer serializer)
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
                target.ValueEditor = new ParameterValueEditor();

                // move the 'view' property
                jobject["editor"] = new JObject { ["view"] = jobject["view"] };
                jobject.Property("view").Remove();
            }

            base.Deserialize(jobject, target, serializer);
        }
    }
}
