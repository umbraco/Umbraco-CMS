using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Umbraco.Web.Models.ContentEditing
{
    /// <summary>
    /// A model representing a new sort order for a content/media item
    /// </summary>
    [DataContract(Name = "content", Namespace = "")]
    public class ContentSortOrder
    {
        /// <summary>
        /// The parent Id of the nodes being sorted
        /// </summary>
        /// <remarks>
        /// This is nullable because currently we don't require this for media sorting
        /// </remarks>
        [DataMember(Name = "parentId")]
        public int? ParentId { get; set; }

        /// <summary>
        /// An array of integer Ids representing the sort order
        /// </summary>
        /// <remarks>
        /// Of course all of these Ids should be at the same level in the heirarchy!!
        /// </remarks>
        [DataMember(Name = "idSortOrder")]
        public int[] IdSortOrder { get; set; }

    }
    
}
