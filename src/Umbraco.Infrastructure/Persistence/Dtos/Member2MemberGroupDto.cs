using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(Constants.DatabaseSchema.Tables.Member2MemberGroup)]
[PrimaryKey("Member", AutoIncrement = false)]
[ExplicitColumns]
internal class Member2MemberGroupDto
{
    [Column("Member")]
    [PrimaryKeyColumn(AutoIncrement = false, Name = "PK_cmsMember2MemberGroup", OnColumns = "Member, MemberGroup")]
    [ForeignKey(typeof(MemberDto))]
    public int Member { get; set; }

    [Column("MemberGroup")]
    [ForeignKey(typeof(NodeDto))]
    public int MemberGroup { get; set; }
}
