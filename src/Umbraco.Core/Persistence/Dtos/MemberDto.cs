using NPoco;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace Umbraco.Core.Persistence.Dtos
{
    [TableName(TableName)]
    [PrimaryKey("nodeId", AutoIncrement = false)]
    [ExplicitColumns]
    internal class MemberDto
    {
        private const string TableName = Constants.DatabaseSchema.Tables.Member;

        [Column("nodeId")]
        [PrimaryKeyColumn(AutoIncrement = false)]
        [ForeignKey(typeof(ContentDto))]
        public int NodeId { get; set; }

        [Column("Email")]
        [Length(1000)]
        [Constraint(Default = "''")]
        public string Email { get; set; }

        [Column("LoginName")]
        [Length(1000)]
        [Constraint(Default = "''")]
        [Index(IndexTypes.NonClustered, Name = "IX_cmsMember_LoginName")]
        public string LoginName { get; set; }

        [Column("Password")]
        [Length(1000)]
        [Constraint(Default = "''")]
        public string Password { get; set; }

        [ResultColumn]
        [Reference(ReferenceType.OneToOne, ReferenceMemberName = "NodeId")]
        public ContentDto ContentDto { get; set; }

        [ResultColumn]
        [Reference(ReferenceType.OneToOne, ReferenceMemberName = "NodeId")]
        public ContentVersionDto ContentVersionDto { get; set; }
    }
}
