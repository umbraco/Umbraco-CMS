using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace Umbraco.Core.Models.Rdbms
{
    [TableName("cmsMember2MemberGroup")]
    [PrimaryKey("Member", autoIncrement = false)]
    [ExplicitColumns]
    internal class Member2MemberGroupDto
    {
        [Column("Member")]
        [PrimaryKeyColumn(AutoIncrement = false, Name = "PK_cmsMember2MemberGroup", OnColumns = "[Member], [MemberGroup]")]
        public int Member { get; set; }

        [Column("MemberGroup")]
        public int MemberGroup { get; set; }
    }
}