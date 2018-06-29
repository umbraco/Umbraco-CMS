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
        private static readonly ConcurrentDictionary<string, DtoMapModel> PropertyInfoCacheInstance = new ConcurrentDictionary<string, DtoMapModel>();

        internal override ConcurrentDictionary<string, DtoMapModel> PropertyInfoCache => PropertyInfoCacheInstance;

        protected override void BuildMap()
        {
            CacheMap<Member, NodeDto>(src => src.Id, dto => dto.NodeId);
            CacheMap<Member, NodeDto>(src => src.CreateDate, dto => dto.CreateDate);
            CacheMap<Member, NodeDto>(src => ((IUmbracoEntity)src).Level, dto => dto.Level);
            CacheMap<Member, NodeDto>(src => ((IUmbracoEntity)src).ParentId, dto => dto.ParentId);
            CacheMap<Member, NodeDto>(src => ((IUmbracoEntity)src).Path, dto => dto.Path);
            CacheMap<Member, NodeDto>(src => ((IUmbracoEntity)src).SortOrder, dto => dto.SortOrder);
            CacheMap<Member, NodeDto>(src => ((IUmbracoEntity)src).CreatorId, dto => dto.UserId);
            CacheMap<Member, NodeDto>(src => src.Name, dto => dto.Text);
            CacheMap<Member, NodeDto>(src => src.Trashed, dto => dto.Trashed);
            CacheMap<Member, NodeDto>(src => src.Key, dto => dto.UniqueId);
            CacheMap<Member, ContentDto>(src => src.ContentTypeId, dto => dto.ContentTypeId);
            CacheMap<Member, ContentTypeDto>(src => src.ContentTypeAlias, dto => dto.Alias);
            CacheMap<Member, ContentVersionDto>(src => src.UpdateDate, dto => dto.VersionDate);

            CacheMap<Member, MemberDto>(src => src.Email, dto => dto.Email);
            CacheMap<Member, MemberDto>(src => src.Username, dto => dto.LoginName);
            CacheMap<Member, MemberDto>(src => src.RawPasswordValue, dto => dto.Password);

            CacheMap<Member, PropertyDataDto>(src => src.IsApproved, dto => dto.IntegerValue);
            CacheMap<Member, PropertyDataDto>(src => src.IsLockedOut, dto => dto.IntegerValue);
            CacheMap<Member, PropertyDataDto>(src => src.Comments, dto => dto.TextValue);
            CacheMap<Member, PropertyDataDto>(src => src.RawPasswordAnswerValue, dto => dto.VarcharValue);
            CacheMap<Member, PropertyDataDto>(src => src.PasswordQuestion, dto => dto.VarcharValue);
            CacheMap<Member, PropertyDataDto>(src => src.FailedPasswordAttempts, dto => dto.IntegerValue);
            CacheMap<Member, PropertyDataDto>(src => src.LastLockoutDate, dto => dto.DateValue);
            CacheMap<Member, PropertyDataDto>(src => src.LastLoginDate, dto => dto.DateValue);
            CacheMap<Member, PropertyDataDto>(src => src.LastPasswordChangeDate, dto => dto.DateValue);

            /* Internal experiment */
            CacheMap<Member, PropertyDataDto>(src => src.DateTimePropertyValue, dto => dto.DateValue);
            CacheMap<Member, PropertyDataDto>(src => src.IntegerPropertyValue, dto => dto.IntegerValue);
            CacheMap<Member, PropertyDataDto>(src => src.BoolPropertyValue, dto => dto.IntegerValue);
            CacheMap<Member, PropertyDataDto>(src => src.LongStringPropertyValue, dto => dto.TextValue);
            CacheMap<Member, PropertyDataDto>(src => src.ShortStringPropertyValue, dto => dto.VarcharValue);
            CacheMap<Member, PropertyTypeDto>(src => src.PropertyTypeAlias, dto => dto.Alias);
        }
    }
}
