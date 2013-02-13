using Umbraco.Core.Models.Membership;
using Umbraco.Core.Models.Rdbms;

namespace Umbraco.Core.Persistence.Factories
{
    internal class UserFactory : IEntityFactory<IUser, UserDto>
    {
        private readonly IUserType _userType;

        public UserFactory(IUserType userType)
        {
            _userType = userType;
        }

        #region Implementation of IEntityFactory<IUser,UserDto>

        public IUser BuildEntity(UserDto dto)
        {
            //TODO Add list of applications for user
            return new User(_userType)
                       {
                           Id = dto.Id,
                           ProfileId = dto.Id,
                           StartContentId = dto.ContentStartId,
                           StartMediaId = dto.MediaStartId.HasValue ? dto.MediaStartId.Value : -1,
                           Password = dto.Password,
                           Username = dto.Login,
                           Name = dto.UserName,
                           IsLockedOut = dto.Disabled,
                           IsApproved = dto.Disabled == false,
                           Email = dto.Email,
                           Language = dto.UserLanguage,
                           DefaultToLiveEditing = dto.DefaultToLiveEditing,
                           NoConsole = dto.NoConsole,
                           Permissions = dto.DefaultPermissions
                       };
        }

        public UserDto BuildDto(IUser entity)
        {
            var dto = new UserDto
                          {
                              ContentStartId = entity.StartContentId,
                              MediaStartId = entity.StartMediaId,
                              DefaultToLiveEditing = entity.DefaultToLiveEditing,
                              Disabled = entity.IsApproved == false,
                              Email = entity.Email,
                              Login = entity.Username,
                              NoConsole = entity.NoConsole,
                              Password = entity.Password,
                              UserLanguage = entity.Language,
                              UserName = entity.Name,
                              Type = short.Parse(entity.UserType.Id.ToString()),
                              DefaultPermissions = entity.Permissions
                          };

            if (entity.HasIdentity)
                dto.Id = entity.Id.SafeCast<int>();

            return dto;
        }

        #endregion
    }
}