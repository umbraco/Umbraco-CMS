using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Models.Rdbms;

namespace Umbraco.Core.Persistence.Factories
{
    internal static class UserFactory 
    {
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
                user.StartContentIds = dto.UserStartNodeDtos.Where(x => x.StartNodeType == (int)UserStartNodeDto.StartNodeTypeValue.Content).Select(x => x.StartNode).ToArray();
                user.StartMediaIds = dto.UserStartNodeDtos.Where(x => x.StartNodeType == (int) UserStartNodeDto.StartNodeTypeValue.Media).Select(x => x.StartNode).ToArray();
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
                UpdateDate = entity.UpdateDate
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