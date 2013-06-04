using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Umbraco.Web.Models.ContentEditing
{
    /// <summary>
    /// A basic version of a content type
    /// </summary>
    /// <remarks>
    /// Generally used to return the minimal amount of data about a content type
    /// </remarks> 
    [DataContract(Name = "contentType", Namespace = "")]
    public class ContentTypeBasic
    {
        [DataMember(Name = "id", IsRequired = true)]
        [Required]
        public int Id { get; set; }

        [DataMember(Name = "name", IsRequired = true)]
        [Required]
        public string Name { get; set; }

        [DataMember(Name = "alias", IsRequired = true)]
        [Required]
        public string Alias { get; set; }

        [DataMember(Name = "description")]
        public string Description { get; set; }

        public string Icon { get; set; }
    }
}