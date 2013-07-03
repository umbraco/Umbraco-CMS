using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Models.Rdbms;

namespace Umbraco.Core.Persistence.Mappers
{
    /// <summary>
    /// Represents a <see cref="UserType"/> to DTO mapper used to translate the properties of the public api 
    /// implementation to that of the database's DTO as sql: [tableName].[columnName].
    /// </summary>
    [MapperFor(typeof(IUserType))]
    [MapperFor(typeof(UserType))]
    public sealed class UserTypeMapper : BaseMapper
    {
        private static readonly ConcurrentDictionary<string, DtoMapModel> PropertyInfoCacheInstance = new ConcurrentDictionary<string, DtoMapModel>();

        //NOTE: its an internal class but the ctor must be public since we're using Activator.CreateInstance to create it
        // otherwise that would fail because there is no public constructor.
        public UserTypeMapper()
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
            CacheMap<UserType, UserTypeDto>(src => src.Id, dto => dto.Id);
            CacheMap<UserType, UserTypeDto>(src => src.Alias, dto => dto.Alias);
            CacheMap<UserType, UserTypeDto>(src => src.Name, dto => dto.Name);
            CacheMap<UserType, UserTypeDto>(src => src.Permissions, dto => dto.DefaultPermissions);
        }


        #endregion
    }
}