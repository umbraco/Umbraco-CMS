using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

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
        
    }
}