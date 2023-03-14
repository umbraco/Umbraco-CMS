using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Persistence.Mappers;

[MapperFor(typeof(IUser))]
[MapperFor(typeof(User))]
public sealed class UserMapper : BaseMapper
{
    public UserMapper(Lazy<ISqlContext> sqlContext, MapperConfigurationStore maps)
        : base(sqlContext, maps)
    {
    }

    protected override void DefineMaps()
    {
        DefineMap<User, UserDto>(nameof(User.Id), nameof(UserDto.Id));
        DefineMap<User, UserDto>(nameof(User.Email), nameof(UserDto.Email));
        DefineMap<User, UserDto>(nameof(User.Username), nameof(UserDto.Login));
        DefineMap<User, UserDto>(nameof(User.RawPasswordValue), nameof(UserDto.Password));
        DefineMap<User, UserDto>(nameof(User.Name), nameof(UserDto.UserName));

        // NOTE: This column in the db is *not* used!
        // DefineMap<User, UserDto>(nameof(User.DefaultPermissions), nameof(UserDto.DefaultPermissions));
        DefineMap<User, UserDto>(nameof(User.IsApproved), nameof(UserDto.Disabled));
        DefineMap<User, UserDto>(nameof(User.IsLockedOut), nameof(UserDto.NoConsole));
        DefineMap<User, UserDto>(nameof(User.Language), nameof(UserDto.UserLanguage));
        DefineMap<User, UserDto>(nameof(User.CreateDate), nameof(UserDto.CreateDate));
        DefineMap<User, UserDto>(nameof(User.UpdateDate), nameof(UserDto.UpdateDate));
        DefineMap<User, UserDto>(nameof(User.LastLockoutDate), nameof(UserDto.LastLockoutDate));
        DefineMap<User, UserDto>(nameof(User.LastLoginDate), nameof(UserDto.LastLoginDate));
        DefineMap<User, UserDto>(nameof(User.LastPasswordChangeDate), nameof(UserDto.LastPasswordChangeDate));
        DefineMap<User, UserDto>(nameof(User.SecurityStamp), nameof(UserDto.SecurityStampToken));
    }
}
