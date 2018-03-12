using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace Umbraco.Core.Models.Rdbms
{
    [TableName("cmsMemberType")]
    [PrimaryKey("pk")]
    [ExplicitColumns]
    internal class MemberTypeDto
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
}
