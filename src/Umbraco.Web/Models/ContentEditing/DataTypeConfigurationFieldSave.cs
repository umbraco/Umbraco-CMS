using System.Runtime.Serialization;

namespace Umbraco.Web.Models.ContentEditing
{
    /// <summary>
    /// Represents a datatype configuration field model for editing.
    /// </summary>
    [DataContract(Name = "preValue", Namespace = "")]
    public class DataTypeConfigurationFieldSave
    {
        /// <summary>
        /// Gets the configuration field key.
        /// </summary>
        [DataMember(Name = "key", IsRequired = true)]
        public string Key { get; set; }

        /// <summary>
        /// Gets the configuration field value.
        /// </summary>
        [DataMember(Name = "value", IsRequired = true)]
        public object Value { get; set; } // fixme - what's a value?
    }
}
