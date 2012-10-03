using Umbraco.Core.Persistence;

namespace Umbraco.Core.Models.Rdbms
{
    [TableName("cmsMember2MemberGroup")]
    [PrimaryKey("Member", autoIncrement = false)]
    [ExplicitColumns]
    internal class Member2MemberGroupDto
    {
        [Column("Memeber")]
        public int Member { get; set; }

        [Column("MemberGroup")]
        public int MemberGroup { get; set; }
    }
}