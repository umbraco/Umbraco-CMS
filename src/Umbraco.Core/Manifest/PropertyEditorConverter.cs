using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Serialization;

namespace Umbraco.Core.Manifest
{
    /// <summary>
    /// Used to convert a property editor manifest to a property editor object
    /// </summary>
    internal class PropertyEditorConverter : JsonCreationConverter<PropertyEditor>
    {
        protected override PropertyEditor Create(Type objectType, JObject jObject)
        {
            return new PropertyEditor();
        }

        protected override void Deserialize(JObject jObject, PropertyEditor target, JsonSerializer serializer)
        {
            if (jObject["editor"] != null)
            {
                //since it's a manifest editor, we need to create it's instance.
                //we need to specify the view value for the editor here otherwise we'll get an exception.
                target.ManifestDefinedValueEditor = new ValueEditor
                    {
                        View = jObject["editor"]["view"].ToString()
                    };
                
            }
            if (jObject["preValueEditor"] != null)
            {
                target.ManifestDefinedPreValueEditor = new PreValueEditor();
            }

            base.Deserialize(jObject, target, serializer);
        }
    }
}