using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Umbraco.Cms.ManagementApi.ViewModels.Move;

/// <summary>
///     A model representing a model for moving or copying
/// </summary>
[DataContract(Name = "content", Namespace = "")]
public class MoveViewModel
{
    /// <summary>
    ///     The Id of the node to move or copy to
    /// </summary>
    [DataMember(Name = "parentId", IsRequired = true)]
    [Required]
    public int ParentId { get; set; }

    /// <summary>
    ///     The id of the node to move or copy
    /// </summary>
    [DataMember(Name = "id", IsRequired = true)]
    [Required]
    public int Id { get; set; }

    /// <summary>
    ///     Boolean indicating whether copying the object should be recursive
    /// </summary>
    [DataMember(Name = "recursive", IsRequired = true)]
    [Required]
    public bool Recursive { get; set; }
}
