using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(TableName)]
[PrimaryKey(PrimaryKeyName, AutoIncrement = false)]
[ExplicitColumns]
internal sealed class Member2MemberGroupDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.Member2MemberGroup;
    public const string PrimaryKeyName = "Member";
    public const string MemberGroupName = "MemberGroup";

    [Column(PrimaryKeyName)]
    [PrimaryKeyColumn(AutoIncrement = false, Name = "PK_cmsMember2MemberGroup", OnColumns = $"{PrimaryKeyName}, {MemberGroupName}")]
    [ForeignKey(typeof(MemberDto))]
    public int Member { get; set; }

    [Column(MemberGroupName)]
    [ForeignKey(typeof(NodeDto))]
    public int MemberGroup { get; set; }
}
