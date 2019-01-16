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
            field.Description = "Add, remove or sort colors.";
            //change the label
            field.Name = "Colors";
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
            var items = dictionary.Where(x => x.Key != "useLabel")
                                  .OrderBy(x => x.Value.SortOrder);

            var items2 = new Dictionary<int, object>();
            foreach (var item in items)
            {
                var valueItem = new ColorPickerColor
                {
                    Color = item.Value.Value,
                    Label = item.Value.Value,
                    SortOrder = item.Value.SortOrder
                };

                if (item.Value.Value.DetectIsJson())
                {
                    try
                    {
                        var valueObject = JsonConvert.DeserializeObject<ColorPickerColor>(item.Value.Value);
                        valueItem = new ColorPickerColor
                        {
                            Color = valueObject.Color,
                            Label = valueObject.Label,
                            SortOrder = valueObject.SortOrder
                        };
                    }
                    catch
                    {
                        // let's say parsing Json failed, we'll not do anything,
                        // we'll just use the valueItem we built in the  first place
                    }
                }

                items2[item.Value.Id] = new JObject
                {
                    { "value", valueItem.Color },
                    { "label", valueItem.Label },
                    { "sortOrder", valueItem.SortOrder }
                };
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
                var useLabel = false;
                if (editorValue.TryGetValue("useLabel", out var useLabelObj))
                {
                    useLabel = useLabelObj is string && (string) useLabelObj == "1";
                    result["useLabel"] = new PreValue(useLabel ? "1" : "0");
                }

                // get all non-empty values
                var index = 0;
                // items get submitted in the sorted order, so just count them up
                var sortOrder = -1;
                foreach (var preValue in val.OfType<JObject>()
                    .Where(x => x["value"] != null)
                    .Select(x =>
                    {
                        var idString = x["id"] == null ? "0" : x["id"].ToString();
                        int.TryParse(idString, out var id);

                        var color = x["value"].ToString();
                        if (string.IsNullOrWhiteSpace(color)) return null;

                        var label = x["label"].ToString();

                        sortOrder++;

                        var value = JsonConvert.SerializeObject(new { value = color, label = label, sortOrder = sortOrder });

                        return new PreValue(id, value, sortOrder);
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

    internal class ColorPickerColor
    {
        [JsonProperty("value")]
        public string Color { get; set; }
        [JsonProperty("label")]
        public string Label { get; set; }
        [JsonProperty("sortOrder")]
        public int SortOrder { get; set; }
    }
}
