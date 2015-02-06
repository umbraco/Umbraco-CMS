using System.ComponentModel;
using System.Runtime.Serialization;

namespace Umbraco.Web.Models.ContentEditing
{
    /// <summary>
    /// The basic data type information
    /// </summary>
    [DataContract(Name = "dataType", Namespace = "")]
    public class DataTypeBasic : EntityBasic
    {
        /// <summary>
        /// Whether or not this is a system data type, in which case it cannot be deleted
        /// </summary>
        [DataMember(Name = "isSystem")]
        [ReadOnly(true)]
        public bool IsSystemDataType { get; set; }
    }
}