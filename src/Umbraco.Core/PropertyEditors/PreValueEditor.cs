using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Umbraco.Core.Models;

namespace Umbraco.Core.PropertyEditors
{
    /// <summary>
    /// Defines a pre-value editor
    /// </summary>
    /// <remarks>
    /// A pre-value editor is made up of multiple pre-value fields, each field defines a key that the value is stored against.
    /// Each field can have any editor and the value from each field can store any data such as a simple string or a json structure. 
    /// 
    /// The Json serialization attributes are required for manifest property editors to work.
    /// </remarks>
    public class PreValueEditor
    {
        public PreValueEditor()
        {
            Fields = Enumerable.Empty<PreValueField>();        
        }

        /// <summary>
        /// A collection of pre-value fields to be edited
        /// </summary>
        /// <remarks>
        /// If fields are specified then the master View and Validators will be ignored
        /// </remarks>
        [JsonProperty("fields")]
        public IEnumerable<PreValueField> Fields { get; set; }

        /// <summary>
        /// A method to format the posted values from the editor to the values to be persisted
        /// </summary>
        /// <param name="editorValue"></param>
        /// <param name="currentValue">
        /// The current value that has been persisted to the database for this pre-value editor. This value may be usesful for 
        /// how the value then get's deserialized again to be re-persisted. In most cases it will probably not be used.
        /// </param>
        /// <returns></returns>
        /// <remarks>
        /// By default this will just return the Posted editorValue.
        /// 
        /// This can be overridden if perhaps you have a comma delimited string posted value but want to convert those to individual rows, or to convert
        /// a json structure to multiple rows.
        /// </remarks>
        public virtual IDictionary<string, string> FormatDataForPersistence(IDictionary<string, string> editorValue, PreValueCollection currentValue)
        {
            return editorValue;
        }

        /// <summary>
        /// This can be used to re-format the currently saved pre-values that will be passed to the editor,
        /// by default this returns the merged default and persisted pre-values.
        /// </summary>
        /// <param name="defaultPreVals">
        /// The default/static pre-vals for the property editor
        /// </param>
        /// <param name="persistedPreVals">
        /// The persisted pre-vals for the property editor
        /// </param>
        /// <returns></returns>
        /// <remarks>
        /// This is generally not going to be used by anything unless a property editor wants to change the merging
        /// functionality or needs to convert some legacy persisted data, or convert the string values to strongly typed values in json (i.e. booleans)
        /// </remarks>
        public virtual IDictionary<string, object> FormatDataForEditor(IDictionary<string, object> defaultPreVals, PreValueCollection persistedPreVals)
        {
            if (defaultPreVals == null)
            {
                defaultPreVals = new Dictionary<string, object>();
            }

            if (persistedPreVals.IsDictionaryBased)
            {
                //we just need to merge the dictionaries now, the persisted will replace default.
                foreach (var item in persistedPreVals.PreValuesAsDictionary)
                {
                    defaultPreVals[item.Key] = item.Value;
                }
                return defaultPreVals;
            }

            //it's an array so need to format it 
            var result = new Dictionary<string, object>();
            var asArray = persistedPreVals.PreValuesAsArray.ToArray();
            for (var i = 0; i < asArray.Length; i++)
            {
                result.Add(i.ToInvariantString(), asArray[i]);
            }
            return result;
        } 

    }
}