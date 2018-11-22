using System.Runtime.Serialization;

namespace Umbraco.Web.Models.ContentEditing
{
    /// <summary>
    /// An object representing the property type validation settings
    /// </summary>
    [DataContract(Name = "propertyValidation", Namespace = "")]
    public class PropertyTypeValidation
    {
        [DataMember(Name = "mandatory")]
        public bool Mandatory { get; set; }

        [DataMember(Name = "pattern")]
        public string Pattern { get; set; }
    }
}