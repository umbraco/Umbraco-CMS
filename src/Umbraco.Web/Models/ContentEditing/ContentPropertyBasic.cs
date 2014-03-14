using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.Models.ContentEditing
{
    /// <summary>
    /// Represents a content property to be saved
    /// </summary>
    [DataContract(Name = "property", Namespace = "")]
    public class ContentPropertyBasic
    {
        [DataMember(Name = "id", IsRequired = true)]
        [Required]
        public int Id { get; set; }

        [DataMember(Name = "value")]
        public object Value { get; set; }

        [DataMember(Name = "alias", IsRequired = true)]
        [Required(AllowEmptyStrings = false)]
        public string Alias { get; set; }

        [DataMember(Name = "editor", IsRequired = false)]
        public string Editor { get; set; }


        /// <summary>
        /// Used internally during model mapping
        /// </summary>
        [IgnoreDataMember]
        internal PropertyEditor PropertyEditor { get; set; }

    }
}