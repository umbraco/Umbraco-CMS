// Copyright (c) Umbraco.
// See LICENSE for more details.

using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(TableName)]
[PrimaryKey([ExternalMemberColumnName, MemberGroupColumnName], AutoIncrement = false)]
[ExplicitColumns]
internal sealed class ExternalMember2MemberGroupDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.ExternalMember2MemberGroup;
    public const string ExternalMemberColumnName = "externalMemberId";

    private const string MemberGroupColumnName = "memberGroupId";

    /// <summary>
    /// Gets or sets the identifier of the external member.
    /// </summary>
    [Column(ExternalMemberColumnName)]
    [PrimaryKeyColumn(AutoIncrement = false, Name = "PK_" + TableName, OnColumns = $"{ExternalMemberColumnName}, {MemberGroupColumnName}")]
    [ForeignKey(typeof(ExternalMemberDto))]
    public int ExternalMemberId { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the member group.
    /// </summary>
    [Column(MemberGroupColumnName)]
    [ForeignKey(typeof(NodeDto))]
    public int MemberGroupId { get; set; }
}
