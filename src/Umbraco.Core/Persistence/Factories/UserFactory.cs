using System;
using System.Collections.Generic;
using System.Globalization;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Models.Rdbms;

namespace Umbraco.Core.Persistence.Factories
{
    internal class UserFactory 
    {
        #region Implementation of IEntityFactory<IUser,UserDto>

        public static IUser BuildEntity(UserDto dto)
        {
            var guidId = dto.Id.ToGuid();

            var groupAliases = new HashSet<string>();
            var allowedSections = new HashSet<string>();
            var userGroups = dto.UserGroupDtos;
            foreach (var userGroup in userGroups)
            {
                groupAliases.Add(userGroup.Alias);
                foreach (var section in userGroup.UserGroup2AppDtos)
                {
                    allowedSections.Add(section.AppAlias);
                }
            }

            var user = new User(dto.Id, dto.UserName, dto.Email, dto.Login,dto.Password, allowedSections, groupAliases);

            try
            {
                user.DisableChangeTracking();
                
                user.Key = guidId;
                user.StartContentId = dto.ContentStartId;
                user.StartMediaId = dto.MediaStartId ?? -1;
                user.IsLockedOut = dto.NoConsole;
                user.IsApproved = dto.Disabled == false;
                user.Language = dto.UserLanguage;
                user.SecurityStamp = dto.SecurityStampToken;
                user.FailedPasswordAttempts = dto.FailedLoginAttempts ?? 0;
                user.LastLockoutDate = dto.LastLockoutDate ?? DateTime.MinValue;
                user.LastLoginDate = dto.LastLoginDate ?? DateTime.MinValue;
                user.LastPasswordChangeDate = dto.LastPasswordChangeDate ?? DateTime.MinValue;

                //on initial construction we don't want to have dirty properties tracked
                // http://issues.umbraco.org/issue/U4-1946
                user.ResetDirtyProperties(false);

                return user;
            }
            finally
            {
                user.EnableChangeTracking();
            }
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
                SecurityStampToken = entity.SecurityStamp,
                FailedLoginAttempts = entity.FailedPasswordAttempts,
                LastLockoutDate = entity.LastLockoutDate == DateTime.MinValue ? (DateTime?)null : entity.LastLockoutDate,
                LastLoginDate = entity.LastLoginDate == DateTime.MinValue ? (DateTime?)null : entity.LastLoginDate,
                LastPasswordChangeDate = entity.LastPasswordChangeDate == DateTime.MinValue ? (DateTime?)null : entity.LastPasswordChangeDate,
            };

            if (entity.HasIdentity)
            {
                dto.Id = entity.Id.SafeCast<int>();
            }

            return dto;
        }

        #endregion
    }
}