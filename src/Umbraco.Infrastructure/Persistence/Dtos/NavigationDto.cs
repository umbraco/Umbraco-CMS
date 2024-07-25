using NPoco;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

// Used internally for representing the data needed for constructing the in-memory navigation structure.
[TableName(NodeDto.TableName)]
internal class NavigationDto
{
    /// <summary>
    ///     Gets the integer identifier of the entity.
    /// </summary>
    [Column("id")]
    public int Id { get; set; }

    /// <summary>
    ///     Gets the Guid unique identifier of the entity.
    /// </summary>
    [Column("uniqueId")]
    public Guid Key { get; set; }

    /// <summary>
    ///     Gets the integer identifier of the parent entity.
    /// </summary>
    [Column("parentId")]
    public int ParentId { get; set; }

    /// <summary>
    ///     Gets a value indicating whether this entity is in the recycle bin.
    /// </summary>
    [Column("trashed")]
    public bool Trashed { get; set; }
}
