using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Umbraco.Core.Manifest;

namespace Umbraco.Core.PropertyEditors
{
    /// <summary>
    /// Represents a datatype configuration field for editing.
    /// </summary>
    public class DataTypeConfigurationField
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DataTypeConfigurationField"/> class.
        /// </summary>
        public DataTypeConfigurationField()
            : this(new List<IPropertyValidator>())
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataTypeConfigurationField"/> class.
        /// </summary>
        public DataTypeConfigurationField(params IPropertyValidator[] validators)
            : this(validators.ToList())
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataTypeConfigurationField"/> class.
        /// </summary>
        private DataTypeConfigurationField(List<IPropertyValidator> validators)
        {
            Validators = validators;
            Config = new Dictionary<string, object>();

            // fill details from attribute, if any
            var attribute = GetType().GetCustomAttribute<DataTypeConfigurationFieldAttribute>(false);
            if (attribute == null) return;

            Name = attribute.Name;
            Description = attribute.Description;
            HideLabel = attribute.HideLabel;
            Key = attribute.Key;
            View = attribute.View;
        }

        /// <summary>
        /// Gets or sets the name of the field.
        /// </summary>
        [JsonProperty("label", Required = Required.Always)]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the field.
        /// </summary>
        [JsonProperty("description")]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to hide the label of the field.
        /// </summary>
        [JsonProperty("hideLabel")]
        public bool HideLabel { get; set; }

        /// <summary>
        /// Gets or sets the key of the field.
        /// </summary>
        [JsonProperty("key", Required = Required.Always)]
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the view to used in the editor.
        /// </summary>
        /// <remarks>
        /// <para>Can be the full virtual path, or the relative path to the Umbraco folder,
        /// or a simple view name which will map to ~/Views/PreValueEditors/{view}.html.</para>
        /// </remarks>
        [JsonProperty("view", Required = Required.Always)]
        public string View { get; set; }

        /// <summary>
        /// Gets the validators of the field.
        /// </summary>
        [JsonProperty("validation", ItemConverterType = typeof(ManifestValidatorConverter))]
        public List<IPropertyValidator> Validators { get; private set; }

        /// <summary>
        /// Gets or sets extra configuration properties for the editor.
        /// </summary>
        [JsonProperty("config")]
        public IDictionary<string, object> Config { get; set; }
    }
}
