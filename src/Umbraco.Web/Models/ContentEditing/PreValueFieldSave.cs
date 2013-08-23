using System.Runtime.Serialization;

namespace Umbraco.Web.Models.ContentEditing
{
    /// <summary>
    /// Defines a pre value editable field for a data type
    /// </summary>
    [DataContract(Name = "preValue", Namespace = "")]
    public class PreValueFieldSave
    {
        /// <summary>
        /// The key to store the pre-value against
        /// </summary>
        [DataMember(Name = "key", IsRequired = true)]
        public string Key { get; set; }

        /// <summary>
        /// The value stored for the pre-value field
        /// </summary>
        [DataMember(Name = "value", IsRequired = true)]
        public object Value { get; set; }
    }
}