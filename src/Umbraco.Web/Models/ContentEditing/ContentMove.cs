using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Umbraco.Web.Models.ContentEditing
{
    /// <summary>
    /// A model representing a new sort order for a content/media item
    /// </summary>
    [DataContract(Name = "content", Namespace = "")]
    public class ContentMove
    {
        /// <summary>
        /// The Id of the node to move or copy to
        /// </summary>
        [DataMember(Name = "parentId", IsRequired = true)]
        [Required]
        public int ParentId { get; set; }

        /// <summary>
        /// ´The id of the node to move or copy
        /// </summary>
        [DataMember(Name = "id", IsRequired = true)]
        [Required]
        public int Id { get; set; }
    }

}
