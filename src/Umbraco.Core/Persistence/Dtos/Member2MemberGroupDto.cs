using NPoco;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace Umbraco.Core.Persistence.Dtos
{
    [TableName(TableName)]
    [PrimaryKey("Member", AutoIncrement = false)]
    [ExplicitColumns]
    internal class Member2MemberGroupDto
    {
        public const string TableName = Constants.DatabaseSchema.Tables.Member2MemberGroup;

        [Column("Member")]
        [PrimaryKeyColumn(AutoIncrement = false, Name = "PK_cmsMember2MemberGroup", OnColumns = "Member, MemberGroup")]
        [ForeignKey(typeof(MemberDto))]
        public int Member { get; set; }

        [Column("MemberGroup")]
        [ForeignKey(typeof(NodeDto))]
        [Index(IndexTypes.NonClustered, Name = "IX_" + TableName + "_MemberGroup")]
        public int MemberGroup { get; set; }
    }
}
