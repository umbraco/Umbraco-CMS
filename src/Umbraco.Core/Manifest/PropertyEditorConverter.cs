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
                target.ManifestDefinedPropertyValueEditor = new PropertyValueEditor
                    {
                        View = jObject["editor"]["view"].ToString()
                    };

                //the manifest JSON is a simplified json for the validators which is actually a dictionary, however, the
                //c# model requires an array of validators not a dictionary so we need to change the json to an array 
                //to deserialize properly.
                JArray converted;
                if (TryConvertValidatorDictionaryToArray(jObject["editor"]["validation"] as JObject, out converted))
                {
                    jObject["editor"]["validation"] = converted;
                }

            }
            if (jObject["prevalues"] != null)
            {
                target.ManifestDefinedPreValueEditor = new PreValueEditor();

                //the manifest JSON is a simplified json for the validators which is actually a dictionary, however, the
                //c# model requires an array of validators not a dictionary so we need to change the json to an array 
                //to deserialize properly.
                var fields = jObject["prevalues"]["fields"] as JArray;
                if (fields != null)
                {
                    foreach (var f in fields)
                    {
                        JArray converted;
                        if (TryConvertValidatorDictionaryToArray(f["validation"] as JObject, out converted))
                        {
                            f["validation"] = converted;
                        }
                    }
                }
            }

            base.Deserialize(jObject, target, serializer);
        }

        private bool TryConvertValidatorDictionaryToArray(JObject validation, out JArray result)
        {
            if (validation == null)
            {
                result = null;
                return false;
            }

            result = new JArray();
            foreach (var entry in validation)
            {
                //in a special case if the value is simply 'true' (boolean) this just indicates that the 
                // validator is enabled, the config should just be empty.
                var formattedItem = JObject.FromObject(new { type = entry.Key, config = entry.Value });
                if (entry.Value.Type == JTokenType.Boolean)
                {
                    formattedItem["config"] = "";
                }

                result.Add(formattedItem);
            }
            return true;
        }
    }
}