using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace Umbraco.Core.Models.Rdbms
{
    [TableName("cmsMember")]
    [PrimaryKey("nodeId", autoIncrement = false)]
    [ExplicitColumns]
    internal class MemberDto
    {
        [Column("nodeId")]
        [PrimaryKeyColumn(AutoIncrement = false)]
        [ForeignKey(typeof(ContentDto), Column = "nodeId")]
        [ForeignKey(typeof(NodeDto))]
        public int NodeId { get; set; }

        [Column("Email")]
        [Length(1000)]
        [Constraint(Default = "''")]
        public string Email { get; set; }

        [Column("LoginName")]
        [Length(1000)]
        [Constraint(Default = "''")]
        public string LoginName { get; set; }

        [Column("Password")]
        [Length(1000)]
        [Constraint(Default = "''")]
        public string Password { get; set; }

        [ResultColumn]
        public ContentVersionDto ContentVersionDto { get; set; }
    }
}