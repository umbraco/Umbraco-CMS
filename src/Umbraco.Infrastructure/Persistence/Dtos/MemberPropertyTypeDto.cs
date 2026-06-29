using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(TableName)]
[PrimaryKey(PrimaryKeyColumnName)]
[ExplicitColumns]
internal sealed class MemberPropertyTypeDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.MemberPropertyType;
    public const string PrimaryKeyColumnName = Constants.DatabaseSchema.Columns.PrimaryKeyNamePk;
    public const string NodeIdColumnName = "NodeId";

    /// <summary>
    /// Gets or sets the primary key of the member property type.
    /// </summary>
    [Column(PrimaryKeyColumnName)]
    [PrimaryKeyColumn]
    public int PrimaryKey { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the node associated with this member property type.
    /// </summary>
    [Column(NodeIdColumnName)]
    [ForeignKey(typeof(NodeDto))]
    [ForeignKey(typeof(ContentTypeDto), Column = ContentTypeDto.NodeIdColumnName)]
    public int NodeId { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the property type.
    /// </summary>
    [Column("propertytypeId")]
    public int PropertyTypeId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this property type is editable by the member.
    /// </summary>
    [Column("memberCanEdit")]
    [Constraint(Default = "0")]
    public bool CanEdit { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this member property type should be visible on the member's profile.
    /// </summary>
    [Column("viewOnProfile")]
    [Constraint(Default = "0")]
    public bool ViewOnProfile { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the member property type is sensitive.
    /// </summary>
    [Column("isSensitive")]
    [Constraint(Default = "0")]
    public bool IsSensitive { get; set; }
}
