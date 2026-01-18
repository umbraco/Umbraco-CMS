using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(TableName)]
[PrimaryKey(PrimaryKeyColumnName, AutoIncrement = false)]
[ExplicitColumns]
internal sealed class Member2MemberGroupDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.Member2MemberGroup;
    public const string PrimaryKeyColumnName = "Member";

    private const string MemberGroupColumnName = "MemberGroup";

    [Column(PrimaryKeyColumnName)]
    [PrimaryKeyColumn(AutoIncrement = false, Name = "PK_cmsMember2MemberGroup", OnColumns = $"{PrimaryKeyColumnName}, {MemberGroupColumnName}")]
    [ForeignKey(typeof(MemberDto))]
    public int Member { get; set; }

    [Column(MemberGroupColumnName)]
    [ForeignKey(typeof(NodeDto))]
    public int MemberGroup { get; set; }
}
