using NPoco;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

internal sealed class AxisDefintionDto
{
    /// <summary>
    /// Gets or sets the unique identifier of the node.
    /// </summary>
    [Column("nodeId")]
    public int NodeId { get; set; }

    /// <summary>
    /// Gets or sets the alias for this axis definition.
    /// </summary>
    [Column("alias")]
    public string? Alias { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the parent entity.
    /// </summary>
    [Column("ParentID")]
    public int ParentId { get; set; }
}
