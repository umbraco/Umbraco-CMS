using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    internal class ColorListPreValueEditor : ValueListPreValueEditor
    {
      
        public ColorListPreValueEditor()
        {
            var field = Fields.First();

            //use a custom editor too
            field.View = "views/propertyeditors/colorpicker/colorpicker.prevalues.html";
            //change the description
            field.Description = "Add and remove colors";
            //change the label
            field.Name = "Add color";
            //need to have some custom validation happening here
            field.Validators.Add(new ColorListValidator());

            Fields.Add(new PreValueField()
            {
                Name = "Use Label to Identify Color?",
                View = "boolean",
                Key = "useLabel",
                Description = "This will store a color as a JSON object rather than a hex string. This will give you access to both the hex value and the color label."
            });
        }

        public override IDictionary<string, object> ConvertDbToEditor(IDictionary<string, object> defaultPreVals,
            PreValueCollection persistedPreVals)
        {
            var dictionary = persistedPreVals.FormatAsDictionary();
            var arrayOfVals = dictionary.Where(x => !"useLabel".InvariantEquals(x.Key))
                .Select(item => item.Value).ToList();
            var result = new Dictionary<string, object>
            {
                { "items", arrayOfVals.ToDictionary(x => x.Id, x => x.Value) }
            };

            // Old code stores colors with their hex value, like this: "000000"
            // New code stores colors as JSON, like this: {"value": "000000", "label": "Black"}
            ConvertItemsToJsonIfDetected(result);

            // Also, need to store boolean that indicates whether or not to use the color labels.
            if (dictionary.ContainsKey("useLabel"))
            {
                result["useLabel"] = dictionary["useLabel"].Value;
            }

            return result;
        }

        public override IDictionary<string, PreValue> ConvertEditorToDb(
            IDictionary<string, object> editorValue, PreValueCollection currentValue)
        {
            var val = editorValue["items"] as JArray;
            var result = new Dictionary<string, PreValue>();

            if (val == null)
            {
                return result;
            }

            try
            {

                object useLabel = null;
                if (editorValue.TryGetValue("useLabel", out useLabel))
                {
                    var strUseLabel = useLabel == null ? "0" : useLabel.ToString();
                    result["useLabel"] = new PreValue(strUseLabel);
                }

                var index = 0;

                //get all values in the array that are not empty 
                foreach (var item in val.OfType<JObject>()
                    .Where(jItem => jItem["value"] != null)
                    .Select(jItem => new
                    {
                        idAsString = jItem["id"] == null ? "0" : jItem["id"].ToString(),
                        valAsString = ValueLabelAsJson(jItem["value"].ToString(), jItem["label"].ToString())
                    })
                    .Where(x => x.valAsString.IsNullOrWhiteSpace() == false))
                {
                    var id = 0;
                    if (!int.TryParse(item.idAsString, out id))
                    {
                        id = 0;
                    }
                    result.Add(index.ToInvariantString(), new PreValue(id, item.valAsString));
                    index++;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error<ValueListPreValueEditor>("Could not deserialize the posted value: " + val, ex);
            }

            return result;
        }

        private static string ValueLabelAsJson(string value, string label)
        {
            return JsonConvert.SerializeObject(new {
                value = value,
                label = label
            });
        }

        internal class ColorListValidator : IPropertyValidator
        {
            public IEnumerable<ValidationResult> Validate(object value, PreValueCollection preValues, PropertyEditor editor)
            {
                var json = value as JArray;
                if (json == null) yield break;

                //validate each item which is a json object
                for (var index = 0; index < json.Count; index++)
                {
                    var i = json[index];
                    var jItem = i as JObject;
                    if (jItem == null || jItem["value"] == null) continue;

                    //NOTE: we will be removing empty values when persisting so no need to validate
                    var asString = jItem["value"].ToString();
                    if (asString.IsNullOrWhiteSpace()) continue;

                    if (Regex.IsMatch(asString, "^([0-9a-f]{3}|[0-9a-f]{6})$", RegexOptions.IgnoreCase) == false)
                    {
                        yield return new ValidationResult("The value " + asString + " is not a valid hex color", new[]
                            {
                                //we'll make the server field the index number of the value so it can be wired up to the view
                                "item_" + index.ToInvariantString()
                            });
                    }
                }
            }
        }
    }
}