using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models.ContentEditing
{
    /// <summary>
    /// A model representing a model for moving
    /// </summary>
    [DataContract(Name = "dictionary", Namespace = "")]
    public class DictionaryMove
    {
        /// <summary>
        /// The Id of the node to move to
        /// </summary>
        [DataMember(Name = "parentId", IsRequired = true)]
        [Required]
        public int ParentId { get; set; }

        /// <summary>
        /// The id of the node to move
        /// </summary>
        [DataMember(Name = "id", IsRequired = true)]
        [Required]
        public int Id { get; set; }
    }
}
