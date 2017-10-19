﻿using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Models.Rdbms;

namespace Umbraco.Core.Persistence.Factories
{
    internal static class UserGroupFactory
    {
        public static IUserGroup BuildEntity(UserGroupDto dto)
        {
            var userGroup = new UserGroup(dto.UserCount, dto.Alias, dto.Name,
                dto.DefaultPermissions.IsNullOrWhiteSpace()
                    ? Enumerable.Empty<string>()
                    : dto.DefaultPermissions.ToCharArray().Select(x => x.ToString(CultureInfo.InvariantCulture)).ToList(),
                dto.Icon);

            try
            {
                userGroup.DisableChangeTracking();
                userGroup.Id = dto.Id;                
                userGroup.CreateDate = dto.CreateDate;
                userGroup.UpdateDate = dto.UpdateDate;                
                userGroup.StartContentId = dto.StartContentId;                
                userGroup.StartMediaId = dto.StartMediaId;                
                if (dto.UserGroup2AppDtos != null)
                {
                    foreach (var app in dto.UserGroup2AppDtos)
                    {
                        userGroup.AddAllowedSection(app.AppAlias);
                    }
                }

                userGroup.ResetDirtyProperties(false);
                return userGroup;
            }
            finally
            {
                userGroup.EnableChangeTracking();
            }
        }

        public static UserGroupDto BuildDto(IUserGroup entity)
        {
            var dto = new UserGroupDto
            {
                Alias = entity.Alias,
                DefaultPermissions = entity.Permissions == null ? "" : string.Join("", entity.Permissions),
                Name = entity.Name,
                UserGroup2AppDtos = new List<UserGroup2AppDto>(),
                CreateDate = entity.CreateDate,
                UpdateDate = entity.UpdateDate,
                Icon = entity.Icon,
                StartMediaId = entity.StartMediaId,
                StartContentId = entity.StartContentId
            };

            foreach (var app in entity.AllowedSections)
            {
                var appDto = new UserGroup2AppDto
                {
                    AppAlias = app
                };
                if (entity.HasIdentity)
                {
                    appDto.UserGroupId = entity.Id;
                }

                dto.UserGroup2AppDtos.Add(appDto);
            }

            if (entity.HasIdentity)
                dto.Id = short.Parse(entity.Id.ToString());

            return dto;
        }

    }
}