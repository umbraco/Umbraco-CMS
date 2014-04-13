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
using umbraco;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// Pre-value editor used to create a list of items
    /// </summary>
    /// <remarks>
    /// This pre-value editor is shared with editors like drop down, checkbox list, etc....
    /// </remarks>
    internal class ValueListPreValueEditor : PreValueEditor
    {

        public ValueListPreValueEditor()
        {
            Fields.AddRange(CreatePreValueFields());
        }

        /// <summary>
        /// Creates the pre-value fields
        /// </summary>
        /// <returns></returns>
        protected List<PreValueField> CreatePreValueFields()
        {
            return new List<PreValueField>
                {
                    new PreValueField(new EnsureUniqueValuesValidator())
                        {
                            Description = "Add and remove values for the list",
                            //we're going to call this 'items' because we are going to override the 
                            //serialization of the pre-values to ensure that each one gets saved with it's own key 
                            //(new db row per pre-value, thus to maintain backwards compatibility)

                            //It's also important to note that by default the dropdown angular controller is expecting the 
                            // config options to come in with a property called 'items'
                            Key = "items",
                            Name = ui.Text("editdatatype", "addPrevalue"),
                            View = "multivalues"
                        }                   
                };
        } 

        /// <summary>
        /// The editor is expecting a json array for a field with a key named "items" so we need to format the persisted values
        /// to this format to be used in the editor.
        /// </summary>
        /// <param name="defaultPreVals"></param>
        /// <param name="persistedPreVals"></param>
        /// <returns></returns>
        public override IDictionary<string, object> ConvertDbToEditor(IDictionary<string, object> defaultPreVals, PreValueCollection persistedPreVals)
        {
            var dictionary = persistedPreVals.FormatAsDictionary();
            var arrayOfVals = dictionary.Select(item => item.Value)
                //ensure they are sorted - though this isn't too important because in json in may not maintain
                // the sorted order and the js has to sort anyways.
                .OrderBy(x => x.SortOrder)
                .ToList();
          
            //the items list will be a dictionary of it's id -> value we need to use the id for persistence for backwards compatibility
            return new Dictionary<string, object> {{"items", arrayOfVals.ToDictionary(x => x.Id, x => PreValueAsDictionary(x))}};
        }

        /// <summary>
        /// Formats the prevalue as a dictionary (as we need to return not just the value, but also the sort-order, to the client)
        /// </summary>
        /// <param name="preValue">The prevalue to format</param>
        /// <returns>Dictionary object containing the prevalue formatted with the field names as keys and the value of those fields as the values</returns>
        private IDictionary<string, object> PreValueAsDictionary(PreValue preValue)
        {
            return new Dictionary<string, object>() { { "value", preValue.Value }, {"sortOrder", preValue.SortOrder } };
        }

        /// <summary>
        /// Need to format the delimited posted string to individual values
        /// </summary>
        /// <param name="editorValue"></param>
        /// <param name="currentValue"></param>
        /// <returns>
        /// A string/string dictionary since all values that need to be persisted in the database are strings.
        /// </returns>
        /// <remarks>
        /// This is mostly because we want to maintain compatibility with v6 drop down property editors that store their prevalues in different db rows.
        /// </remarks>
        public override IDictionary<string, PreValue> ConvertEditorToDb(IDictionary<string, object> editorValue, PreValueCollection currentValue)
        {
            var val = editorValue["items"] as JArray;
            var result = new Dictionary<string, PreValue>();
            
            if (val == null)
            {
                return result;
            }

            try
            {
                var index = 0;

                //get all values in the array that are not empty 
                foreach (var item in val.OfType<JObject>()
                    .Where(jItem => jItem["value"] != null)
                    .Select(jItem => new
                        {
                            idAsString = jItem["id"] == null ? "0" : jItem["id"].ToString(), 
                            valAsString = jItem["value"].ToString()
                        })
                    .Where(x => x.valAsString.IsNullOrWhiteSpace() == false))
                {
                    var id = 0;
                    int.TryParse(item.idAsString, out id);
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

        /// <summary>
        /// A custom validator to ensure that all values in the list are unique
        /// </summary>
        internal class EnsureUniqueValuesValidator : IPropertyValidator
        {
            public IEnumerable<ValidationResult> Validate(object value, PreValueCollection preValues, PropertyEditor editor)
            {
                var json = value as JArray;
                if (json == null) yield break;

                //get all values in the array that are not empty (we'll remove empty values when persisting anyways)
                var groupedValues = json.OfType<JObject>()
                                        .Where(jItem => jItem["value"] != null)
                                        .Select((jItem, index) => new {value = jItem["value"].ToString(), index = index})
                                        .Where(asString => asString.value.IsNullOrWhiteSpace() == false)
                                        .GroupBy(x => x.value);
                
                foreach (var g in groupedValues.Where(g => g.Count() > 1))
                {
                    yield return new ValidationResult("The value " + g.Last().value + " must be unique", new[]
                        {
                            //we'll make the server field the index number of the value so it can be wired up to the view
                            "item_" + g.Last().index.ToInvariantString()
                        });
                }


            }
        }
    }
}
