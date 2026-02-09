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

    [Column(PrimaryKeyColumnName)]
    [PrimaryKeyColumn]
    public int PrimaryKey { get; set; }

    [Column(NodeIdColumnName)]
    [ForeignKey(typeof(NodeDto))]
    [ForeignKey(typeof(ContentTypeDto), Column = ContentTypeDto.NodeIdColumnName)]
    public int NodeId { get; set; }

    [Column("propertytypeId")]
    public int PropertyTypeId { get; set; }

    [Column("memberCanEdit")]
    [Constraint(Default = "0")]
    public bool CanEdit { get; set; }

    [Column("viewOnProfile")]
    [Constraint(Default = "0")]
    public bool ViewOnProfile { get; set; }

    [Column("isSensitive")]
    [Constraint(Default = "0")]
    public bool IsSensitive { get; set; }
}
