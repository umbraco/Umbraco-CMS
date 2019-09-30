using System;
using System.Linq;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Persistence.Dtos;

namespace Umbraco.Core.Persistence.Factories
{
    internal static class UserFactory
    {
        public static IUser BuildEntity(UserDto dto)
        {
            var guidId = dto.Id.ToGuid();

            var user = new User(dto.Id, dto.UserName, dto.Email, dto.Login,dto.Password,
                dto.UserGroupDtos.Select(x => x.ToReadOnlyGroup()).ToArray(),
                dto.UserStartNodeDtos.Where(x => x.StartNodeType == (int)UserStartNodeDto.StartNodeTypeValue.Content).Select(x => x.StartNode).ToArray(),
                dto.UserStartNodeDtos.Where(x => x.StartNodeType == (int)UserStartNodeDto.StartNodeTypeValue.Media).Select(x => x.StartNode).ToArray());

            try
            {
                user.DisableChangeTracking();

                user.Key = guidId;
                user.IsLockedOut = dto.NoConsole;
                user.IsApproved = dto.Disabled == false;
                user.Language = dto.UserLanguage;
                user.SecurityStamp = dto.SecurityStampToken;
                user.FailedPasswordAttempts = dto.FailedLoginAttempts ?? 0;
                user.LastLockoutDate = dto.LastLockoutDate ?? DateTime.MinValue;
                user.LastLoginDate = dto.LastLoginDate ?? DateTime.MinValue;
                user.LastPasswordChangeDate = dto.LastPasswordChangeDate ?? DateTime.MinValue;
                user.CreateDate = dto.CreateDate;
                user.UpdateDate = dto.UpdateDate;
                user.Avatar = dto.Avatar;
                user.EmailConfirmedDate = dto.EmailConfirmedDate;
                user.InvitedDate = dto.InvitedDate;
                user.TourData = dto.TourData;

                // we should never get user with ID zero from database, except
                // when upgrading from v7 - mark that user so that we do not
                // save it back to database (as that would create a *new* user)
                // see also: UserRepository.PersistNewItem
                if (dto.Id == 0)
                    user.AdditionalData["IS_V7_ZERO"] = true;

                // reset dirty initial properties (U4-1946)
                user.ResetDirtyProperties(false);

                return user;
            }
            finally
            {
                user.EnableChangeTracking();
            }
        }

        public static UserDto BuildDto(IUser entity)
        {
            var dto = new UserDto
            {
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
                CreateDate = entity.CreateDate,
                UpdateDate = entity.UpdateDate,
                Avatar = entity.Avatar,
                EmailConfirmedDate = entity.EmailConfirmedDate,
                InvitedDate = entity.InvitedDate,
                TourData = entity.TourData
            };

            foreach (var startNodeId in entity.StartContentIds)
            {
                dto.UserStartNodeDtos.Add(new UserStartNodeDto
                {
                    StartNode = startNodeId,
                    StartNodeType = (int)UserStartNodeDto.StartNodeTypeValue.Content,
                    UserId = entity.Id
                });
            }

            foreach (var startNodeId in entity.StartMediaIds)
            {
                dto.UserStartNodeDtos.Add(new UserStartNodeDto
                {
                    StartNode = startNodeId,
                    StartNodeType = (int)UserStartNodeDto.StartNodeTypeValue.Media,
                    UserId = entity.Id
                });
            }

            if (entity.HasIdentity)
            {
                dto.Id = entity.Id.SafeCast<int>();
            }

            return dto;
        }
    }
}
