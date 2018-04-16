using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Core;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    internal class ColorPickerConfigurationEditor : ConfigurationEditor<ColorPickerConfiguration>
    {
        public ColorPickerConfigurationEditor()
        {
            var items = Fields.First(x => x.Key == "items");

            // customize the items field
            items.View = "views/propertyeditors/colorpicker/colorpicker.prevalues.html";
            items.Description = "Add and remove colors";
            items.Name = "Add color";
            items.Validators.Add(new ColorListValidator());
        }

        public override Dictionary<string, object> ToConfigurationEditor(ColorPickerConfiguration configuration)
        {
            var items = configuration?.Items.ToDictionary(x => x.Id.ToString(), x => GetItemValue(x, configuration.UseLabel)) ?? new object();
            var useLabel = configuration?.UseLabel ?? false;

            return new Dictionary<string, object>
            {
                { "items", items },
                { "useLabel", useLabel }
            };
        }

        private object GetItemValue(ValueListConfiguration.ValueListItem item, bool useLabel)
        {
            if (useLabel)
            {
                return item.Value.DetectIsJson()
                    ? JsonConvert.DeserializeObject(item.Value)
                    : new JObject { { "color", item.Value }, { "label", item.Value } };
            }

            if (!item.Value.DetectIsJson())
                return item.Value;

            var jobject = (JObject) JsonConvert.DeserializeObject(item.Value);
            return jobject.Property("color").Value.Value<string>();
        }

        public override ColorPickerConfiguration FromConfigurationEditor(IDictionary<string, object> editorValues, ColorPickerConfiguration configuration)
        {
            var output = new ColorPickerConfiguration();

            if (!editorValues.TryGetValue("items", out var jjj) || !(jjj is JArray jItems))
                return output; // oops

            // handle useLabel
            if (editorValues.TryGetValue("useLabel", out var useLabelObj))
                output.UseLabel = useLabelObj.TryConvertTo<bool>();

            // auto-assigning our ids, get next id from existing values
            var nextId = 1;
            if (configuration?.Items != null && configuration.Items.Count > 0)
                nextId = configuration.Items.Max(x => x.Id) + 1;

            // create ValueListItem instances - sortOrder is ignored here
            foreach (var item in jItems.OfType<JObject>())
            {
                var value = item.Property("value")?.Value?.Value<string>();
                if (string.IsNullOrWhiteSpace(value)) continue;

                var id = item.Property("id")?.Value?.Value<int>() ?? 0;
                if (id >= nextId) nextId = id + 1;

                // if using a label, replace color by json blob
                // (a pity we have to serialize here!)
                if (output.UseLabel)
                {
                    var label = item.Property("label")?.Value?.Value<string>();
                    value = JsonConvert.SerializeObject(new { value, label });
                }

                output.Items.Add(new ValueListConfiguration.ValueListItem { Id = id, Value = value });
            }

            // ensure ids
            foreach (var item in output.Items)
                if (item.Id == 0)
                    item.Id = nextId++;

            return output;
        }

        internal class ColorListValidator : IValueValidator
        {
            public IEnumerable<ValidationResult> Validate(object value, string valueType, object dataTypeConfiguration)
            {
                if (!(value is JArray json)) yield break;

                //validate each item which is a json object
                for (var index = 0; index < json.Count; index++)
                {
                    var i = json[index];
                    if (!(i is JObject jItem) || jItem["value"] == null) continue;

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
