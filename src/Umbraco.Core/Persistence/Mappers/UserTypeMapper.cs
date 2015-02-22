using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence.SqlSyntax;

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

        #region Overrides of BaseMapper

        public UserTypeMapper(ISqlSyntaxProvider sqlSyntax) : base(sqlSyntax)
        {
        }

        internal override ConcurrentDictionary<string, DtoMapModel> PropertyInfoCache
        {
            get { return PropertyInfoCacheInstance; }
        }

        protected override void BuildMap()
        {
            CacheMap<UserType, UserTypeDto>(src => src.Id, dto => dto.Id);
            CacheMap<UserType, UserTypeDto>(src => src.Alias, dto => dto.Alias);
            CacheMap<UserType, UserTypeDto>(src => src.Name, dto => dto.Name);
            CacheMap<UserType, UserTypeDto>(src => src.Permissions, dto => dto.DefaultPermissions);
        }


        #endregion
    }
}