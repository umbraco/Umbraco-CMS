using System;
using System.Globalization;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Models.Rdbms;

namespace Umbraco.Core.Persistence.Factories
{
    internal class UserFactory 
    {
        #region Implementation of IEntityFactory<IUser,UserDto>

        public IUser BuildEntity(UserDto dto)
        {
            var guidId = dto.Id.ToGuid();
            var user = new User();

            try
            {
                user.DisableChangeTracking();

                user.Id = dto.Id;
                user.Key = guidId;
                user.StartContentId = dto.ContentStartId;
                user.StartMediaId = dto.MediaStartId.HasValue ? dto.MediaStartId.Value : -1;
                user.RawPasswordValue = dto.Password;
                user.Username = dto.Login;
                user.Name = dto.UserName;
                user.IsLockedOut = dto.NoConsole;
                user.IsApproved = dto.Disabled == false;
                user.Email = dto.Email;
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