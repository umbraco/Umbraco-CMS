using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.ManagementApi.ViewModels.Move;

/// <summary>
///     A model representing a model for moving or copying
/// </summary>
public class MoveViewModel
{
    /// <summary>
    ///     The Id of the node to move or copy to
    /// </summary>
    [Required]
    public int ParentId { get; set; }

    /// <summary>
    ///     The id of the node to move or copy
    /// </summary>
    [Required]
    public int Id { get; set; }
}
