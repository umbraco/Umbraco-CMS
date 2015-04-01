using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Models.Rdbms;

namespace Umbraco.Core.Persistence.Factories
{
    internal class UserFactory 
    {
        private readonly IUserType _userType;

        public UserFactory(IUserType userType)
        {
            _userType = userType;
        }

        #region Implementation of IEntityFactory<IUser,UserDto>

        public IUser BuildEntity(UserDto dto)
        {
            var guidId = dto.Id.ToGuid();
            var user = new User(_userType)
                {
                    Id = dto.Id,
                    Key = guidId,
                    StartContentId = dto.ContentStartId,
                    StartMediaId = dto.MediaStartId.HasValue ? dto.MediaStartId.Value : -1,
                    RawPasswordValue = dto.Password,
                    Username = dto.Login,
                    Name = dto.UserName,
                    IsLockedOut = dto.NoConsole,
                    IsApproved = dto.Disabled == false,
                    Email = dto.Email,
                    Language = dto.UserLanguage,
                    //make it a GUID if it's empty
                    SecurityStamp = dto.SecurityStampToken.IsNullOrWhiteSpace() ? Guid.NewGuid().ToString() : dto.SecurityStampToken
                };

            foreach (var app in dto.User2AppDtos)
            {
                user.AddAllowedSection(app.AppAlias);
            }

            //on initial construction we don't want to have dirty properties tracked
            // http://issues.umbraco.org/issue/U4-1946
            user.ResetDirtyProperties(false);

            return user;
        }

        public UserDto BuildDto(IUser entity)
        {
            var dto = new UserDto
                          {
                              ContentStartId = entity.StartContentId,
                              MediaStartId = entity.StartMediaId,
                              Disabled = entity.IsApproved == false,
                              Email = entity.Email,
                              Login = entity.Username,
                              NoConsole = entity.IsLockedOut,
                              Password = entity.RawPasswordValue,
                              UserLanguage = entity.Language,
                              UserName = entity.Name,
                              Type = short.Parse(entity.UserType.Id.ToString(CultureInfo.InvariantCulture)),
                              User2AppDtos = new List<User2AppDto>(),
                              SecurityStampToken = entity.SecurityStamp
                          };

            foreach (var app in entity.AllowedSections)
            {
                var appDto = new User2AppDto
                    {
                        AppAlias = app
                    };
                if (entity.HasIdentity)
                {
                    appDto.UserId = (int) entity.Id;
                }

                dto.User2AppDtos.Add(appDto);
            }

            if (entity.HasIdentity)
                dto.Id = entity.Id.SafeCast<int>();

            return dto;
        }

        #endregion
    }
}