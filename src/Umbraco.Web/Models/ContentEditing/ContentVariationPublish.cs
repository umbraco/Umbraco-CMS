using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Umbraco.Web.Models.ContentEditing
{
    /// <summary>
    /// Used to indicate which additional variants to publish when a content item is published
    /// </summary>
    public class ContentVariationPublish
    {
        [DataMember(Name = "languageId", IsRequired = true)]
        [Required]
        public int LanguageId { get; set; }

        [DataMember(Name = "segment")]
        public string Segment { get; set; }
    }
}
