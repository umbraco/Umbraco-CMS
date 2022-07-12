using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models.ContentEditing;

/// <summary>
///     A model for retrieving multiple content types based on their keys.
/// </summary>
[DataContract(Name = "contentTypes", Namespace = "")]
public class ContentTypesByKeys
{
    /// <summary>
    ///     ID of the parent of the content type.
    /// </summary>
    [DataMember(Name = "parentId")]
    [Required]
    public int ParentId { get; set; }

    /// <summary>
    ///     The id of every content type to get.
    /// </summary>
    [DataMember(Name = "contentTypeKeys")]
    [Required]
    public Guid[]? ContentTypeKeys { get; set; }
}
