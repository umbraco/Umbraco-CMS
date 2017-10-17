using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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

            Fields.Insert(0, new PreValueField
            {
                Name = "Include labels?",
                View = "boolean",
                Key = "useLabel",
                Description = "Stores colors as a Json object containing both the color hex string and label, rather than just the hex string."
            });
        }

        public override IDictionary<string, object> ConvertDbToEditor(IDictionary<string, object> defaultPreVals, PreValueCollection persistedPreVals)
        {
            var dictionary = persistedPreVals.FormatAsDictionary();
            var items = dictionary
                .Where(x => x.Key != "useLabel")
                .ToDictionary(x => x.Value.Id, x => x.Value.Value);

            var items2 = new Dictionary<int, object>();
            foreach (var item in items)
            {
                if (item.Value.DetectIsJson() == false)
                {
                    items2[item.Key] = item.Value;
                    continue;
                }

                try
                {
                    items2[item.Key] = JsonConvert.DeserializeObject(item.Value);
                }
                catch
                {
                    // let's say parsing Json failed, so what we have is the string - build json
                    items2[item.Key] = new JObject { { "color", item.Value }, { "label", item.Value } };
                }
            }

            var result = new Dictionary<string, object> { { "items", items2 } };
            var useLabel = dictionary.ContainsKey("useLabel") && dictionary["useLabel"].Value == "1";
            if (useLabel)
                result["useLabel"] = dictionary["useLabel"].Value;

            return result;
        }

        public override IDictionary<string, PreValue> ConvertEditorToDb(IDictionary<string, object> editorValue, PreValueCollection currentValue)
        {
            var val = editorValue["items"] as JArray;
            var result = new Dictionary<string, PreValue>();
            if (val == null) return result;

            try
            {
                object useLabelObj;
                var useLabel = false;
                if (editorValue.TryGetValue("useLabel", out useLabelObj))
                {
                    useLabel = useLabelObj is string && (string) useLabelObj == "1";
                    result["useLabel"] = new PreValue(useLabel ? "1" : "0");
                }

                // get all non-empty values
                var index = 0;
                foreach (var preValue in val.OfType<JObject>()
                    .Where(x => x["value"] != null)
                    .Select(x =>
                    {
                        var idString = x["id"] == null ? "0" : x["id"].ToString();
                        int id;
                        if (int.TryParse(idString, out id) == false) id = 0;

                        var color = x["value"].ToString();
                        if (string.IsNullOrWhiteSpace(color)) return null;

                        var label = x["label"].ToString();
                        return new PreValue(id, useLabel
                            ? JsonConvert.SerializeObject(new { value = color, label = label })
                            : color);
                    })
                    .WhereNotNull())
                {
                    result.Add(index.ToInvariantString(), preValue);
                    index++;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error<ValueListPreValueEditor>("Could not deserialize the posted value: " + val, ex);
            }

            return result;
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