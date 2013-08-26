using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using umbraco;

namespace Umbraco.Web.PropertyEditors
{
    internal class DropDownPreValueEditor : PreValueEditor
    {

        public DropDownPreValueEditor()
        {
            Fields = CreatePreValueFields();
        }

        /// <summary>
        /// Creates the pre-value fields
        /// </summary>
        /// <returns></returns>
        protected List<PreValueField> CreatePreValueFields()
        {
            return new List<PreValueField>
                {
                    new PreValueField
                        {
                            Description = "Add and remove values for the drop down list",
                            //we're going to call this 'items' because we are going to override the 
                            //serialization of the pre-values to ensure that each one gets saved with it's own key 
                            //(new db row per pre-value, thus to maintain backwards compatibility)

                            //It's also important to note that by default the dropdown angular controller is expecting the 
                            // config options to come in with a property called 'items'
                            Key = "items",
                            Name = ui.Text("editdatatype", "addPrevalue"),
                            View = "Views/PropertyEditors/dropdown/dropdown.prevalue.html"
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
        public override IDictionary<string, object> FormatDataForEditor(IDictionary<string, object> defaultPreVals, PreValueCollection persistedPreVals)
        {
            var dictionary = PreValueCollection.AsDictionary(persistedPreVals);
            var arrayOfVals = dictionary.Select(item => item.Value).ToList();

            //the items list will be a dictionary of it's id -> value we need to use the id for persistence for backwards compatibility
            return new Dictionary<string, object> { { "items", arrayOfVals.ToDictionary(x => x.Id, x => x.Value) } };
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
        public override IDictionary<string, string> FormatDataForPersistence(IDictionary<string, object> editorValue, PreValueCollection currentValue)
        {
            var val = editorValue["items"] as JArray;
            var result = new Dictionary<string, string>();
            
            if (val == null)
            {
                return result;
            }

            try
            {
                var index = 0;
                foreach (var item in val)
                {
                    result.Add(index.ToInvariantString(), item.ToString());
                    index++;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error<DropDownPreValueEditor>("Could not deserialize the posted value: " + val, ex);                
            }

            return result;
        }
    }
}