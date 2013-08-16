using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Umbraco.Core.PropertyEditors
{
    /// <summary>
    /// Defines a pre-value editor
    /// </summary>
    /// <remarks>
    /// The Json serialization attributes are required for manifest property editors to work
    /// </remarks>
    public class PreValueEditor
    {
        public PreValueEditor()
        {
            Fields = Enumerable.Empty<PreValueField>();
            //Validators = Enumerable.Empty<ValidatorBase>();
        }

        ///// <summary>
        ///// The full virtual path or the relative path to the current Umbraco folder for the angular view
        ///// </summary>
        //[JsonProperty("view")]
        //public string View { get; set; }

        ///// <summary>
        ///// A collection of validators for the pre value editor
        ///// </summary>
        //[JsonProperty("validation")]
        //public IEnumerable<ValidatorBase> Validators { get; set; }

        /// <summary>
        /// A collection of pre-value fields to be edited
        /// </summary>
        /// <remarks>
        /// If fields are specified then the master View and Validators will be ignored
        /// </remarks>
        [JsonProperty("fields")]
        public IEnumerable<PreValueField> Fields { get; set; } 

        ///// <summary>
        ///// Returns true if this pre value editor is defined by individual fields
        ///// </summary>
        //public bool IsFieldBased
        //{
        //    get { return Fields.Any(); }
        //}
    }

    /// <summary>
    /// Defines a pre value editable field
    /// </summary>
    public class PreValueField
    {
        /// <summary>
        /// The key to store the pre-value against
        /// </summary>
        [JsonProperty("key", Required = Required.Always)]
        public string Key { get; set; }

        /// <summary>
        /// The view to render for the file
        /// </summary>
        [JsonProperty("view", Required = Required.Always)]
        public string View { get; set; }

        /// <summary>
        /// A collection of validators for the pre value field
        /// </summary>
        [JsonProperty("validation")]
        public IEnumerable<ValidatorBase> Validators { get; set; }
    }
}