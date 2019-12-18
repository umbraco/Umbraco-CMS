using System;
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
        public UserGroupMapper(Lazy<ISqlContext> sqlContext, ConcurrentDictionary<Type, ConcurrentDictionary<string, string>> maps)
            : base(sqlContext, maps)
        { }

        protected override void DefineMaps()
        {
            DefineMap<UserGroup, UserGroupDto>(nameof(UserGroup.Id), nameof(UserGroupDto.Id));
            DefineMap<UserGroup, UserGroupDto>(nameof(UserGroup.Alias), nameof(UserGroupDto.Alias));
            DefineMap<UserGroup, UserGroupDto>(nameof(UserGroup.Name), nameof(UserGroupDto.Name));
            DefineMap<UserGroup, UserGroupDto>(nameof(UserGroup.Icon), nameof(UserGroupDto.Icon));
            DefineMap<UserGroup, UserGroupDto>(nameof(UserGroup.StartContentId), nameof(UserGroupDto.StartContentId));
            DefineMap<UserGroup, UserGroupDto>(nameof(UserGroup.StartMediaId), nameof(UserGroupDto.StartMediaId));
        }
    }
}
