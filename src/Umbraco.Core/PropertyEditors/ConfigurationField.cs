using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Umbraco.Core.IO;

namespace Umbraco.Core.PropertyEditors
{
    /// <summary>
    /// Represents a datatype configuration field for editing.
    /// </summary>
    public class ConfigurationField
    {
        private string _view;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationField"/> class.
        /// </summary>
        public ConfigurationField()
            : this(new List<IValueValidator>())
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationField"/> class.
        /// </summary>
        public ConfigurationField(params IValueValidator[] validators)
            : this(validators.ToList())
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationField"/> class.
        /// </summary>
        private ConfigurationField(List<IValueValidator> validators)
        {
            Validators = validators;
            Config = new Dictionary<string, object>();

            // fill details from attribute, if any
            var attribute = GetType().GetCustomAttribute<ConfigurationFieldAttribute>(false);
            if (attribute == null) return;

            Name = attribute.Name;
            Description = attribute.Description;
            HideLabel = attribute.HideLabel;
            Key = attribute.Key;
            View = attribute.View;
        }

        /// <summary>
        /// Gets or sets the key of the field.
        /// </summary>
        [JsonProperty("key", Required = Required.Always)]
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the name of the field.
        /// </summary>
        [JsonProperty("label", Required = Required.Always)]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the property name of the field.
        /// </summary>
        [JsonIgnore]
        public string PropertyName { get; set; }

        /// <summary>
        /// Gets or sets the property CLR type of the field.
        /// </summary>
        [JsonIgnore]
        public Type PropertyType { get; set; }

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
        /// Gets or sets the view to used in the editor.
        /// </summary>
        /// <remarks>
        /// <para>Can be the full virtual path, or the relative path to the Umbraco folder,
        /// or a simple view name which will map to ~/Views/PreValueEditors/{view}.html.</para>
        /// </remarks>
        [JsonProperty("view", Required = Required.Always)]
        public string View
        {
            get => _view;
            set => _view = IOHelper.ResolveVirtualUrl(value);
        }

        /// <summary>
        /// Gets the validators of the field.
        /// </summary>
        [JsonProperty("validation")]
        public List<IValueValidator> Validators { get; }

        /// <summary>
        /// Gets or sets extra configuration properties for the editor.
        /// </summary>
        [JsonProperty("config")]
        public IDictionary<string, object> Config { get; set; }
    }
}
