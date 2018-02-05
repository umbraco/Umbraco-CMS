using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using Umbraco.Core;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;

namespace Umbraco.Web.PropertyEditors
{
    internal class ColorPickerConfigurationEditor : ValueListConfigurationEditor
    {
        public ColorPickerConfigurationEditor(ILocalizedTextService textService)
            : base(textService)
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
        }

        public override Dictionary<string, object> ToConfigurationEditor(ValueListConfiguration configuration)
        {
            if (configuration == null)
                return new Dictionary<string, object>
                {
                    { "items", new object() }
                };

            // for now, we have to do this, because the color picker is weird, but it's fixed in 7.7 at some point
            // and then we probably don't need this whole override method anymore - base shouls be enough?

            return new Dictionary<string, object>
            {
                { "items", configuration.Items.ToDictionary(x => x.Id.ToString(), x => x.Value) }
            };
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
