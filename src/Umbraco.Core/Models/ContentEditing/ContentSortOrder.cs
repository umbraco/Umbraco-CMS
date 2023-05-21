using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models.ContentEditing;

/// <summary>
///     A model representing a new sort order for a content/media item
/// </summary>
[DataContract(Name = "content", Namespace = "")]
public class ContentSortOrder
{
    /// <summary>
    ///     The parent Id of the nodes being sorted
    /// </summary>
    [DataMember(Name = "parentId", IsRequired = true)]
    [Required]
    public int ParentId { get; set; }

    /// <summary>
    ///     An array of integer Ids representing the sort order
    /// </summary>
    /// <remarks>
    ///     Of course all of these Ids should be at the same level in the hierarchy!!
    /// </remarks>
    [DataMember(Name = "idSortOrder", IsRequired = true)]
    [Required]
    public int[]? IdSortOrder { get; set; }
}
