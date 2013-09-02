using System.Collections.Generic;
using Newtonsoft.Json;
using Umbraco.Core.Manifest;

namespace Umbraco.Core.PropertyEditors
{
    /// <summary>
    /// Defines a pre value editable field
    /// </summary>
    public class PreValueField
    {
        /// <summary>
        /// Standard constructor
        /// </summary>
        public PreValueField()
        {
            Validators = new List<IPropertyValidator>();

            //check for an attribute and fill the values
            var att = GetType().GetCustomAttribute<PreValueFieldAttribute>(false);
            if (att != null)
            {
                Name = att.Name;
                Description = att.Description;
                HideLabel = att.HideLabel;
                Key = att.Key;
                View = att.View;
            }
        }

        /// <summary>
        /// Constructor used to set validators instead of adding them later
        /// </summary>
        /// <param name="validators"></param>
        public PreValueField(params IPropertyValidator[] validators)
            : this()
        {
            foreach (var v in validators)
            {
                Validators.Add(v);
            }
        }

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
        /// Defines the view to use for the editor, this can be one of 3 things:
        /// * the full virtual path or 
        /// * the relative path to the current Umbraco folder 
        /// * a simple view name which will map to the views/prevalueeditors/{view}.html
        /// </summary>
        [JsonProperty("view", Required = Required.Always)]
        public string View { get; set; }

        /// <summary>
        /// A collection of validators for the pre value field
        /// </summary>
        [JsonProperty("validation", ItemConverterType = typeof(ManifestValidatorConverter))]
        public List<IPropertyValidator> Validators { get; private set; }
    }
}