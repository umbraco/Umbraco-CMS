using NPoco;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

internal class AxisDefintionDto
{
    [Column("nodeId")]
    public int NodeId { get; set; }

    [Column("alias")]
    public string? Alias { get; set; }

    [Column("ParentID")]
    public int ParentId { get; set; }
}
