using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Umbraco.Web.Models.ContentEditing
{
    /// <summary>
    /// A model representing a model for sending to translate
    /// </summary>
    [DataContract(Name = "content", Namespace = "")]
    public class SendToTranslate
    {
        /// <summary>
        /// 
        /// </summary>
        [DataMember(Name = "id", IsRequired = true)]
        [Required]
        public int Id { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember(Name = "userId", IsRequired = true)]
        [Required]
        public int UserId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember(Name = "language", IsRequired = true)]
        [Required]
        public string Language { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember(Name = "includeSubPages", IsRequired = true)]
        [Required]
        public bool IncludeSubPages { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember(Name = "comment")]
        public string Comment { get; set; }
    }

}
