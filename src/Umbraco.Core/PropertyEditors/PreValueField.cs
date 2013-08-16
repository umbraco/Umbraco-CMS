using System.Collections.Generic;
using Newtonsoft.Json;

namespace Umbraco.Core.PropertyEditors
{
    /// <summary>
    /// Defines a pre value editable field
    /// </summary>
    public class PreValueField
    {
        /// <summary>
        /// The name to display for this pre-value field
        /// </summary>
        [JsonProperty("label", Required = Required.Always)]
        public string Name { get; set; }

        /// <summary>
        /// The description to display for this pre-value field
        /// </summary>
        [JsonProperty("description")]
        public string Description { get; set; }

        /// <summary>
        /// Specifies whether to hide the label for the pre-value
        /// </summary>
        [JsonProperty("hideLabel")]
        public bool HideLabel { get; set; }

        /// <summary>
        /// The key to store the pre-value against
        /// </summary>
        [JsonProperty("key", Required = Required.Always)]
        public string Key { get; set; }

        /// <summary>
        /// The view to render for the field
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