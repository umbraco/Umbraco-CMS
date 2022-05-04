using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(Constants.DatabaseSchema.Tables.MemberPropertyType)]
[PrimaryKey("pk")]
[ExplicitColumns]
internal class MemberPropertyTypeDto
{
    [Column("pk")]
    [PrimaryKeyColumn]
    public int PrimaryKey { get; set; }

    [Column("NodeId")]
    [ForeignKey(typeof(NodeDto))]
    [ForeignKey(typeof(ContentTypeDto), Column = "nodeId")]
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
