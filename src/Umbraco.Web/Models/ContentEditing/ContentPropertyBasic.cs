using System;
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
        /// <summary>
        /// This is the cmsPropertyData ID
        /// </summary>
        /// <remarks>
        /// This is not really used for anything
        /// </remarks>
        [DataMember(Name = "id", IsRequired = true)]
        [Required]
        public int Id { get; set; }

        [DataMember(Name = "dataTypeId", IsRequired = true)]
        [Required]
        public Guid DataTypeId { get; set; }

        [DataMember(Name = "value")]
        public object Value { get; set; }

        [DataMember(Name = "alias", IsRequired = true)]
        [Required(AllowEmptyStrings = false)]
        public string Alias { get; set; }

        [DataMember(Name = "editor", IsRequired = false)]
        public string Editor { get; set; }

        /// <summary>
        /// Flags the property to denote that it can contain sensitive data
        /// </summary>
        [DataMember(Name = "isSensitive", IsRequired = false)]
        public bool IsSensitive { get; set; }

        /// <summary>
        /// Used internally during model mapping
        /// </summary>
        [IgnoreDataMember]
        internal PropertyEditor PropertyEditor { get; set; }

    }
}
