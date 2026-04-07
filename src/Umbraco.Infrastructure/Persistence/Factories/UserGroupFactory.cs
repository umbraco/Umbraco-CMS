using Umbraco.Cms.Core.Extensions;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Models.Membership.Permissions;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Persistence.Mappers;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Factories;

internal static class UserGroupFactory
{
    /// <summary>
    /// Constructs an <see cref="IUserGroup"/> entity from the provided <see cref="UserGroupDto"/>,
    /// mapping its properties, permissions, allowed sections, and languages. Granular permissions are
    /// mapped using the supplied <paramref name="permissionMappers"/>; unknown types are handled with a fallback.
    /// </summary>
    /// <param name="shortStringHelper">The helper used for processing and normalizing string aliases.</param>
    /// <param name="dto">The data transfer object containing user group data to map from.</param>
    /// <param name="permissionMappers">A dictionary of permission mappers, keyed by context, used to map granular permissions.</param>
    /// <returns>
    /// The constructed <see cref="IUserGroup"/> entity with properties, permissions, allowed sections, languages,
    /// and granular permissions mapped from the DTO.
    /// </returns>
    public static IUserGroup BuildEntity(IShortStringHelper shortStringHelper, UserGroupDto dto, IDictionary<string, IPermissionMapper> permissionMappers)
    {
        var userGroup = new UserGroup(
            shortStringHelper,
            dto.UserCount,
            dto.Alias,
            dto.Name,
            dto.Icon);

        try
        {
            userGroup.DisableChangeTracking();
            userGroup.Id = dto.Id;
            userGroup.Key = dto.Key;
            userGroup.CreateDate = dto.CreateDate.EnsureUtc();
            userGroup.UpdateDate = dto.UpdateDate.EnsureUtc();
            userGroup.StartContentId = dto.StartContentId;
            userGroup.StartMediaId = dto.StartMediaId;
            userGroup.Permissions = dto.UserGroup2PermissionDtos.Select(x => x.Permission).ToHashSet();
            userGroup.HasAccessToAllLanguages = dto.HasAccessToAllLanguages;
            userGroup.Description = dto.Description;
            if (dto.UserGroup2AppDtos != null)
            {
                foreach (UserGroup2AppDto app in dto.UserGroup2AppDtos)
                {
                    userGroup.AddAllowedSection(app.AppAlias);
                }
            }

            foreach (UserGroup2LanguageDto language in dto.UserGroup2LanguageDtos)
            {
                userGroup.AddAllowedLanguage(language.LanguageId);
            }

            foreach (UserGroup2PermissionDto permission in dto.UserGroup2PermissionDtos)
            {
                userGroup.Permissions.Add(permission.Permission);
            }

            foreach (UserGroup2GranularPermissionDto granularPermission in dto.UserGroup2GranularPermissionDtos)
            {
                IGranularPermission toInsert;

                if (permissionMappers.TryGetValue(granularPermission.Context, out var mapper))
                {
                    toInsert = mapper.MapFromDto(granularPermission);
                }
                else
                {
                    toInsert = new UnknownTypeGranularPermission()
                    {
                        Permission = granularPermission.Permission,
                        Context = granularPermission.Context,
                    };
                }

                userGroup.GranularPermissions.Add(toInsert);
            }

            userGroup.ResetDirtyProperties(false);
            return userGroup;
        }
        finally
        {
            userGroup.EnableChangeTracking();
        }
    }

    /// <summary>
    /// Creates a <see cref="UserGroupDto"/> from the specified <see cref="IUserGroup"/> entity.
    /// Copies all basic properties, and populates related collections such as allowed sections and permissions.
    /// </summary>
    /// <param name="entity">The user group entity to convert.</param>
    /// <returns>
    /// A <see cref="UserGroupDto"/> representing the user group, including its allowed sections and permissions.
    /// </returns>
    public static UserGroupDto BuildDto(IUserGroup entity)
    {
        var dto = new UserGroupDto
        {
            Key = entity.Key,
            Alias = entity.Alias,
            Name = entity.Name,
            Description = entity.Description,
            UserGroup2AppDtos = new List<UserGroup2AppDto>(),
            CreateDate = entity.CreateDate,
            UpdateDate = entity.UpdateDate,
            Icon = entity.Icon,
            StartMediaId = entity.StartMediaId,
            StartContentId = entity.StartContentId,
            HasAccessToAllLanguages = entity.HasAccessToAllLanguages,
        };

        foreach (var app in entity.AllowedSections)
        {
            var appDto = new UserGroup2AppDto { AppAlias = app };
            if (entity.HasIdentity)
            {
                appDto.UserGroupId = entity.Id;
            }

            dto.UserGroup2AppDtos.Add(appDto);
        }

        foreach (var permission in entity.Permissions)
        {
            var permissionDto = new UserGroup2PermissionDto { Permission = permission };
            if (entity.HasIdentity)
            {
                permissionDto.UserGroupKey = entity.Key;
            }

            dto.UserGroup2PermissionDtos.Add(permissionDto);
        }

        if (entity.HasIdentity)
        {
            dto.Id = entity.Id;
        }

        return dto;
    }
}
