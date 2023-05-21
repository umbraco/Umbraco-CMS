using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Persistence.Mappers;

/// <summary>
///     Represents a <see cref="Member" /> to DTO mapper used to translate the properties of the public api
///     implementation to that of the database's DTO as sql: [tableName].[columnName].
/// </summary>
[MapperFor(typeof(IMember))]
[MapperFor(typeof(Member))]
public sealed class MemberMapper : BaseMapper
{
    public MemberMapper(Lazy<ISqlContext> sqlContext, MapperConfigurationStore maps)
        : base(sqlContext, maps)
    {
    }

    protected override void DefineMaps()
    {
        DefineMap<Member, NodeDto>(nameof(Member.Id), nameof(NodeDto.NodeId));
        DefineMap<Member, NodeDto>(nameof(Member.CreateDate), nameof(NodeDto.CreateDate));
        DefineMap<Member, NodeDto>(nameof(Member.Level), nameof(NodeDto.Level));
        DefineMap<Member, NodeDto>(nameof(Member.ParentId), nameof(NodeDto.ParentId));
        DefineMap<Member, NodeDto>(nameof(Member.Path), nameof(NodeDto.Path));
        DefineMap<Member, NodeDto>(nameof(Member.SortOrder), nameof(NodeDto.SortOrder));
        DefineMap<Member, NodeDto>(nameof(Member.CreatorId), nameof(NodeDto.UserId));
        DefineMap<Member, NodeDto>(nameof(Member.Name), nameof(NodeDto.Text));
        DefineMap<Member, NodeDto>(nameof(Member.Trashed), nameof(NodeDto.Trashed));
        DefineMap<Member, NodeDto>(nameof(Member.Key), nameof(NodeDto.UniqueId));
        DefineMap<Member, ContentDto>(nameof(Member.ContentTypeId), nameof(ContentDto.ContentTypeId));
        DefineMap<Member, ContentTypeDto>(nameof(Member.ContentTypeAlias), nameof(ContentTypeDto.Alias));
        DefineMap<Member, ContentVersionDto>(nameof(Member.UpdateDate), nameof(ContentVersionDto.VersionDate));

        DefineMap<Member, MemberDto>(nameof(Member.Email), nameof(MemberDto.Email));
        DefineMap<Member, MemberDto>(nameof(Member.Username), nameof(MemberDto.LoginName));
        DefineMap<Member, MemberDto>(nameof(Member.RawPasswordValue), nameof(MemberDto.Password));
        DefineMap<Member, MemberDto>(nameof(Member.IsApproved), nameof(MemberDto.IsApproved));
        DefineMap<Member, MemberDto>(nameof(Member.IsLockedOut), nameof(MemberDto.IsLockedOut));
        DefineMap<Member, MemberDto>(nameof(Member.FailedPasswordAttempts), nameof(MemberDto.FailedPasswordAttempts));
        DefineMap<Member, MemberDto>(nameof(Member.LastLockoutDate), nameof(MemberDto.LastLockoutDate));
        DefineMap<Member, MemberDto>(nameof(Member.LastLoginDate), nameof(MemberDto.LastLoginDate));
        DefineMap<Member, MemberDto>(nameof(Member.LastPasswordChangeDate), nameof(MemberDto.LastPasswordChangeDate));

        DefineMap<Member, PropertyDataDto>(nameof(Member.Comments), nameof(PropertyDataDto.TextValue));

        /* Internal experiment */
        DefineMap<Member, PropertyDataDto>(nameof(Member.DateTimePropertyValue), nameof(PropertyDataDto.DateValue));
        DefineMap<Member, PropertyDataDto>(nameof(Member.IntegerPropertyValue), nameof(PropertyDataDto.IntegerValue));
        DefineMap<Member, PropertyDataDto>(nameof(Member.BoolPropertyValue), nameof(PropertyDataDto.IntegerValue));
        DefineMap<Member, PropertyDataDto>(nameof(Member.LongStringPropertyValue), nameof(PropertyDataDto.TextValue));
        DefineMap<Member, PropertyDataDto>(nameof(Member.ShortStringPropertyValue), nameof(PropertyDataDto.VarcharValue));
        DefineMap<Member, PropertyTypeDto>(nameof(Member.PropertyTypeAlias), nameof(PropertyTypeDto.Alias));
    }
}
