using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using Umbraco.Core;
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
            var fields = CreatePreValueFields();
            //change the description
            fields.First().Description = "Add and remove colors in HEX format without a prefixed '#'";
            //need to have some custom validation happening here
            fields.First().Validators = new List<ValidatorBase>
                {
                    new ColorListValidator()
                };

            Fields = fields;
        }

        internal class ColorListValidator : ValidatorBase
        {
            public override IEnumerable<ValidationResult> Validate(object value, string preValues, PropertyEditor editor)
            {
                var json = value as JArray;
                if (json != null)
                {
                    //validate each item
                    foreach (var i in json)
                    {
                        //NOTE: we will be removing empty values when persisting so no need to validate
                        var asString = i.ToString();
                        if (asString.IsNullOrWhiteSpace() == false)
                        {
                            if (Regex.IsMatch(asString, "^([0-9a-f]{3}|[0-9a-f]{6})$", RegexOptions.IgnoreCase) == false)
                            {
                                yield return new ValidationResult("The value " + asString + " is not a valid hex color", new[]
                                    {
                                        //we'll make the server field name the value of the hex color so we can wire it back up to the
                                        //individual row in the UI.
                                        asString
                                    });
                            }
                        }
                    }
                }
            }
        }
    }
}