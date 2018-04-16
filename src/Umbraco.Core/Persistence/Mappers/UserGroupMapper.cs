using System.Collections.Concurrent;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Persistence.Dtos;

namespace Umbraco.Core.Persistence.Mappers
{
    /// <summary>
    /// Represents a <see cref="UserGroup"/> to DTO mapper used to translate the properties of the public api
    /// implementation to that of the database's DTO as sql: [tableName].[columnName].
    /// </summary>
    [MapperFor(typeof(IUserGroup))]
    [MapperFor(typeof(UserGroup))]
    public sealed class UserGroupMapper : BaseMapper
    {
        private static readonly ConcurrentDictionary<string, DtoMapModel> PropertyInfoCacheInstance = new ConcurrentDictionary<string, DtoMapModel>();

        //NOTE: its an internal class but the ctor must be public since we're using Activator.CreateInstance to create it
        // otherwise that would fail because there is no public constructor.
        public UserGroupMapper()
        {
            BuildMap();
        }

        #region Overrides of BaseMapper

        internal override ConcurrentDictionary<string, DtoMapModel> PropertyInfoCache => PropertyInfoCacheInstance;

        protected override void BuildMap()
        {
            CacheMap<UserGroup, UserGroupDto>(src => src.Id, dto => dto.Id);
            CacheMap<UserGroup, UserGroupDto>(src => src.Alias, dto => dto.Alias);
            CacheMap<UserGroup, UserGroupDto>(src => src.Name, dto => dto.Name);
            CacheMap<UserGroup, UserGroupDto>(src => src.Icon, dto => dto.Icon);
            CacheMap<UserGroup, UserGroupDto>(src => src.StartContentId, dto => dto.StartContentId);
            CacheMap<UserGroup, UserGroupDto>(src => src.StartMediaId, dto => dto.StartMediaId);
        }

        #endregion
    }
}
