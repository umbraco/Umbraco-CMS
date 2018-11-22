using System.Collections.Concurrent;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;

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

        //NOTE: its an internal class but the ctor must be public since we're using Activator.CreateInstance to create it
        // otherwise that would fail because there is no public constructor.
        public MemberMapper()
        {
            BuildMap();
        }

        #region Overrides of BaseMapper

        internal override ConcurrentDictionary<string, DtoMapModel> PropertyInfoCache
        {
            get { return PropertyInfoCacheInstance; }
        }

        internal override void BuildMap()
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
            CacheMap<Member, ContentVersionDto>(src => src.Version, dto => dto.VersionId);

            CacheMap<Member, MemberDto>(src => src.Email, dto => dto.Email);
            CacheMap<Member, MemberDto>(src => src.Username, dto => dto.LoginName);
            CacheMap<Member, MemberDto>(src => src.RawPasswordValue, dto => dto.Password);

            CacheMap<Member, PropertyDataDto>(src => src.IsApproved, dto => dto.Integer);
            CacheMap<Member, PropertyDataDto>(src => src.IsLockedOut, dto => dto.Integer);
            CacheMap<Member, PropertyDataDto>(src => src.Comments, dto => dto.Text);
            CacheMap<Member, PropertyDataDto>(src => src.RawPasswordAnswerValue, dto => dto.VarChar);
            CacheMap<Member, PropertyDataDto>(src => src.PasswordQuestion, dto => dto.VarChar);
            CacheMap<Member, PropertyDataDto>(src => src.FailedPasswordAttempts, dto => dto.Integer);
            CacheMap<Member, PropertyDataDto>(src => src.LastLockoutDate, dto => dto.Date);
            CacheMap<Member, PropertyDataDto>(src => src.LastLoginDate, dto => dto.Date);
            CacheMap<Member, PropertyDataDto>(src => src.LastPasswordChangeDate, dto => dto.Date);

            /* Internal experiment */
            CacheMap<Member, PropertyDataDto>(src => src.DateTimePropertyValue, dto => dto.Date);
            CacheMap<Member, PropertyDataDto>(src => src.IntegerPropertyValue, dto => dto.Integer);
            CacheMap<Member, PropertyDataDto>(src => src.BoolPropertyValue, dto => dto.Integer);
            CacheMap<Member, PropertyDataDto>(src => src.LongStringPropertyValue, dto => dto.Text);
            CacheMap<Member, PropertyDataDto>(src => src.ShortStringPropertyValue, dto => dto.VarChar);
            CacheMap<Member, PropertyTypeDto>(src => src.PropertyTypeAlias, dto => dto.Alias);
        }

        #endregion
    }
}