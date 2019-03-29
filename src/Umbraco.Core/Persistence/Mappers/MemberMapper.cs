using System;
using System.Collections.Concurrent;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Entities;
using Umbraco.Core.Persistence.Dtos;

namespace Umbraco.Core.Persistence.Mappers
{
    /// <summary>
    /// Represents a <see cref="Member"/> to DTO mapper used to translate the properties of the public api
    /// implementation to that of the database's DTO as sql: [tableName].[columnName].
    /// </summary>
    [MapperFor(typeof(IMember))]
    [MapperFor(typeof(Member))]
    public sealed class MemberMapper : BaseMapper
    {
        public MemberMapper(ISqlContext sqlContext, ConcurrentDictionary<Type, ConcurrentDictionary<string, string>> maps)
            : base(sqlContext, maps)
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

            DefineMap<Member, PropertyDataDto>(nameof(Member.IsApproved), nameof(PropertyDataDto.IntegerValue));
            DefineMap<Member, PropertyDataDto>(nameof(Member.IsLockedOut), nameof(PropertyDataDto.IntegerValue));
            DefineMap<Member, PropertyDataDto>(nameof(Member.Comments), nameof(PropertyDataDto.TextValue));
            DefineMap<Member, PropertyDataDto>(nameof(Member.RawPasswordAnswerValue), nameof(PropertyDataDto.VarcharValue));
            DefineMap<Member, PropertyDataDto>(nameof(Member.PasswordQuestion), nameof(PropertyDataDto.VarcharValue));
            DefineMap<Member, PropertyDataDto>(nameof(Member.FailedPasswordAttempts), nameof(PropertyDataDto.IntegerValue));
            DefineMap<Member, PropertyDataDto>(nameof(Member.LastLockoutDate), nameof(PropertyDataDto.DateValue));
            DefineMap<Member, PropertyDataDto>(nameof(Member.LastLoginDate), nameof(PropertyDataDto.DateValue));
            DefineMap<Member, PropertyDataDto>(nameof(Member.LastPasswordChangeDate), nameof(PropertyDataDto.DateValue));

            /* Internal experiment */
            DefineMap<Member, PropertyDataDto>(nameof(Member.DateTimePropertyValue), nameof(PropertyDataDto.DateValue));
            DefineMap<Member, PropertyDataDto>(nameof(Member.IntegerPropertyValue), nameof(PropertyDataDto.IntegerValue));
            DefineMap<Member, PropertyDataDto>(nameof(Member.BoolPropertyValue), nameof(PropertyDataDto.IntegerValue));
            DefineMap<Member, PropertyDataDto>(nameof(Member.LongStringPropertyValue), nameof(PropertyDataDto.TextValue));
            DefineMap<Member, PropertyDataDto>(nameof(Member.ShortStringPropertyValue), nameof(PropertyDataDto.VarcharValue));
            DefineMap<Member, PropertyTypeDto>(nameof(Member.PropertyTypeAlias), nameof(PropertyTypeDto.Alias));
        }
    }
}
