using NPoco;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

// Used internally for representing the data needed for constructing the in-memory navigation structure.
[TableName(NodeDto.TableName)]
internal class NavigationDto : INavigationModel
{
    /// <inheritdoc/>
    [Column(NodeDto.IdColumnName)]
    public int Id { get; set; }

    /// <inheritdoc/>
    [Column(NodeDto.KeyColumnName)]
    public Guid Key { get; set; }

    /// <inheritdoc/>
    [Column(NodeDto.ParentIdColumnName)]
    public int ParentId { get; set; }

    /// <inheritdoc/>
    [Column(NodeDto.TrashedColumnName)]
    public bool Trashed { get; set; }
}
