using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Models.Rdbms;

namespace Umbraco.Core.Persistence.Mappers
{
    internal sealed class UserMapper : BaseMapper
    {
        private static readonly ConcurrentDictionary<string, DtoMapModel> PropertyInfoCache = new ConcurrentDictionary<string, DtoMapModel>();

        internal static UserMapper Instance = new UserMapper();

        private UserMapper()
        {
            BuildMap();
        }

        #region Overrides of BaseMapper

        internal override void BuildMap()
        {
            CacheMap<User, UserDto>(src => src.Id, dto => dto.Id);
            CacheMap<User, UserDto>(src => src.Email, dto => dto.Email);
            CacheMap<User, UserDto>(src => src.Username, dto => dto.Login);
            CacheMap<User, UserDto>(src => src.Password, dto => dto.Password);
            CacheMap<User, UserDto>(src => src.Name, dto => dto.UserName);
            CacheMap<User, UserDto>(src => src.Permissions, dto => dto.DefaultPermissions);
            CacheMap<User, UserDto>(src => src.StartMediaId, dto => dto.MediaStartId);
            CacheMap<User, UserDto>(src => src.StartContentId, dto => dto.ContentStartId);
            CacheMap<User, UserDto>(src => src.DefaultToLiveEditing, dto => dto.DefaultToLiveEditing);
            CacheMap<User, UserDto>(src => src.IsApproved, dto => dto.Disabled);
            CacheMap<User, UserDto>(src => src.NoConsole, dto => dto.NoConsole);
            CacheMap<User, UserDto>(src => src.UserType, dto => dto.Type);
            CacheMap<User, UserDto>(src => src.Lanuguage, dto => dto.UserLanguage);
        }

        internal override string Map(string propertyName)
        {
            if (!PropertyInfoCache.ContainsKey(propertyName))
                return string.Empty;

            var dtoTypeProperty = PropertyInfoCache[propertyName];

            return base.GetColumnName(dtoTypeProperty.Type, dtoTypeProperty.PropertyInfo);
        }

        internal override void CacheMap<TSource, TDestination>(Expression<Func<TSource, object>> sourceMember, Expression<Func<TDestination, object>> destinationMember)
        {
            var property = base.ResolveMapping(sourceMember, destinationMember);
            PropertyInfoCache.AddOrUpdate(property.SourcePropertyName, property, (x, y) => property);
        }

        #endregion
    }
}