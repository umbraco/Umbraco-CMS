using System.Collections.Generic;
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
        /// <summary>
        /// The full virtual path or the relative path to the current Umbraco folder for the angular view
        /// </summary>
        [JsonProperty("view")]
        public string View { get; set; }

        /// <summary>
        /// A collection of validators for the pre value editor
        /// </summary>
        [JsonProperty("validation")]
        public IEnumerable<ValidatorBase> Validators { get; set; }
    }
}