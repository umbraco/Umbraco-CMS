using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models.ContentEditing;

/// <summary>
///     A model for retrieving multiple content types based on their aliases.
/// </summary>
[DataContract(Name = "contentTypes", Namespace = "")]
public class ContentTypesByAliases
{
    /// <summary>
    ///     Id of the parent of the content type.
    /// </summary>
    [DataMember(Name = "parentId")]
    [Required]
    public int ParentId { get; set; }

    /// <summary>
    ///     The alias of every content type to get.
    /// </summary>
    [DataMember(Name = "contentTypeAliases")]
    [Required]
    public string[]? ContentTypeAliases { get; set; }
}
