using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(TableName)]
[PrimaryKey(PrimaryKeyName)]
[ExplicitColumns]
internal sealed class MemberPropertyTypeDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.MemberPropertyType;
    public const string PrimaryKeyName = Constants.DatabaseSchema.PrimaryKeyNamePK;
    public const string NodeIdName = "nodeId";

    [Column(PrimaryKeyName)]
    [PrimaryKeyColumn]
    public int PrimaryKey { get; set; }

    [Column(NodeIdName)]
    [ForeignKey(typeof(NodeDto))]
    [ForeignKey(typeof(ContentTypeDto), Column = NodeIdName)]
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
