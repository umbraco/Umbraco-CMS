using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Models.Rdbms;

namespace Umbraco.Core.Persistence.Mappers
{
    [MapperFor(typeof(IUser))]
    [MapperFor(typeof(User))]
    public sealed class UserMapper : BaseMapper
    {
        private static readonly ConcurrentDictionary<string, DtoMapModel> PropertyInfoCacheInstance = new ConcurrentDictionary<string, DtoMapModel>();

        //NOTE: its an internal class but the ctor must be public since we're using Activator.CreateInstance to create it
        // otherwise that would fail because there is no public constructor.
        public UserMapper()
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
            CacheMap<User, UserDto>(src => src.Id, dto => dto.Id);
            CacheMap<User, UserDto>(src => src.Email, dto => dto.Email);
            CacheMap<User, UserDto>(src => src.Username, dto => dto.Login);
            CacheMap<User, UserDto>(src => src.RawPasswordValue, dto => dto.Password);
            CacheMap<User, UserDto>(src => src.Name, dto => dto.UserName);
            //NOTE: This column in the db is *not* used!
            //CacheMap<User, UserDto>(src => src.DefaultPermissions, dto => dto.DefaultPermissions);         
            CacheMap<User, UserDto>(src => src.IsApproved, dto => dto.Disabled);
            CacheMap<User, UserDto>(src => src.IsLockedOut, dto => dto.NoConsole);
            CacheMap<User, UserDto>(src => src.Language, dto => dto.UserLanguage);
            CacheMap<User, UserDto>(src => src.CreateDate, dto => dto.CreateDate);
            CacheMap<User, UserDto>(src => src.UpdateDate, dto => dto.UpdateDate);
            CacheMap<User, UserDto>(src => src.LastLockoutDate, dto => dto.LastLockoutDate);
            CacheMap<User, UserDto>(src => src.LastLoginDate, dto => dto.LastLoginDate);
            CacheMap<User, UserDto>(src => src.LastPasswordChangeDate, dto => dto.LastPasswordChangeDate);
            CacheMap<User, UserDto>(src => src.SecurityStamp, dto => dto.SecurityStampToken);
        }

        #endregion
    }
}