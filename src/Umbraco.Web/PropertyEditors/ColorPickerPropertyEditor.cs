using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    [PropertyEditor(Constants.PropertyEditors.ColorPicker, "Color Picker", "colorpicker")]
    public class ColorPickerPropertyEditor : PropertyEditor
    {
        /// <summary>
        /// Return a custom pre-value editor
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// We are just going to re-use the ValueListPreValueEditor
        /// </remarks>
        protected override PreValueEditor CreatePreValueEditor()
        {
            return new ColorListPreValueEditor();
        }

    }

    internal class ColorListPreValueEditor : ValueListPreValueEditor
    {
        public ColorListPreValueEditor()
        {
            Fields.AddRange(CreatePreValueFields());
            //use a custom editor too
            Fields.First().View = "views/propertyeditors/colorpicker/colorpicker.prevalues.html";
            //change the description
            Fields.First().Description = "Add and remove colors in HEX format without a prefixed '#'";
            //need to have some custom validation happening here
            Fields.First().Validators.Add(new ColorListValidator());
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