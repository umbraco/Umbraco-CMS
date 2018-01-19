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
            if (jobject.Property("view") != null)
            {
                // the deserializer will first try to get the property, and that would throw since
                // the editor would try to create a new value editor, so we have to set a
                // value editor by ourselves, which will then be populated by the deserializer.
                target.ValueEditor = new ParameterValueEditor();

                // the 'view' property in the manifest is at top-level, and needs to be moved
                // down one level to the actual value editor.
                jobject["editor"] = new JObject { ["view"] = jobject["view"] };
                jobject.Property("view").Remove();
            }

            base.Deserialize(jobject, target, serializer);
        }
    }
}
