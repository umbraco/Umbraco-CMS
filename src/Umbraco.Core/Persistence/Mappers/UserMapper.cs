using System.Collections.Concurrent;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Persistence.Dtos;

namespace Umbraco.Core.Persistence.Mappers
{
    [MapperFor(typeof(IUser))]
    [MapperFor(typeof(User))]
    public sealed class UserMapper : BaseMapper
    {
        private static readonly ConcurrentDictionary<string, DtoMapModel> PropertyInfoCacheInstance = new ConcurrentDictionary<string, DtoMapModel>();

        internal override ConcurrentDictionary<string, DtoMapModel> PropertyInfoCache => PropertyInfoCacheInstance;

        protected override void BuildMap()
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
    }
}
