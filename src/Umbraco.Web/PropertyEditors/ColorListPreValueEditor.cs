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
        }

        public override IDictionary<string, object> ConvertDbToEditor(IDictionary<string, object> defaultPreVals, PreValueCollection persistedPreVals)
        {
            var dictionary = persistedPreVals.FormatAsDictionary();
            var arrayOfVals = dictionary.Select(item => item.Value)
                //ensure they are sorted - though this isn't too important because in json in may not maintain
                // the sorted order and the js has to sort anyways.
                .OrderBy(x => x.SortOrder)
                .ToList();

            return new Dictionary<string, object> { { "items", arrayOfVals.ToDictionary(x => x.Id, x => PreValueAsDictionary(x)) } };
        }

        /// <summary>
        /// Formats the prevalue as a dictionary (as we need to return not just the value, but also the sort-order, to the client)
        /// </summary>
        /// <param name="preValue">The prevalue to format</param>
        /// <returns>Dictionary object containing the prevalue formatted with the field names as keys and the value of those fields as the values</returns>
        private IDictionary<string, object> PreValueAsDictionary(PreValue preValue)
        {
            return new Dictionary<string, object>() { { "value", preValue.Value }, { "sortOrder", preValue.SortOrder } };
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
                    var asString = jItem["value"].Value<string>();
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