using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Umbraco.Web.Models.ContentEditing
{
    /// <summary>
    /// A model representing the configuration for a content type defined as a list container
    /// </summary>
    [DataContract(Name = "config", Namespace = "")]
    public class ContentTypeContainerConfiguration
    {
        /// <summary>
        /// The page size for the list
        /// </summary>
        [DataMember(Name = "pageSize", IsRequired = true)]
        [Required]
        public int PageSize { get; set; }
    }
}
