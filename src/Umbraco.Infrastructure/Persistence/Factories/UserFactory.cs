using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Models.Membership.Permissions;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Persistence.Mappers;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Factories;

internal static class UserFactory
{
    public static IUser BuildEntity(
        GlobalSettings globalSettings,
        UserDto dto,
        IDictionary<string, IPermissionMapper> permissionMappers)
    {
        Guid key = dto.Key;
        // This should only happen if the user is still not migrated to have a true key.
        if (key == Guid.Empty)
        {
            key = dto.Id.ToGuid();
        }

        var user = new User(globalSettings, dto.Id, dto.UserName, dto.Email, dto.Login, dto.Password,
            dto.PasswordConfig,
            dto.UserGroupDtos.Select(x => ToReadOnlyGroup(x, permissionMappers)).ToArray(),
            dto.UserStartNodeDtos.Where(x => x.StartNodeType == (int)UserStartNodeDto.StartNodeTypeValue.Content)
                .Select(x => x.StartNode).ToArray(),
            dto.UserStartNodeDtos.Where(x => x.StartNodeType == (int)UserStartNodeDto.StartNodeTypeValue.Media)
                .Select(x => x.StartNode).ToArray());

        try
        {
            user.DisableChangeTracking();

            user.Key = key;
            user.IsLockedOut = dto.NoConsole;
            user.IsApproved = dto.Disabled == false;
            user.Language = dto.UserLanguage;
            user.SecurityStamp = dto.SecurityStampToken;
            user.FailedPasswordAttempts = dto.FailedLoginAttempts ?? 0;
            user.LastLockoutDate = dto.LastLockoutDate;
            user.LastLoginDate = dto.LastLoginDate;
            user.LastPasswordChangeDate = dto.LastPasswordChangeDate;
            user.CreateDate = dto.CreateDate;
            user.UpdateDate = dto.UpdateDate;
            user.Avatar = dto.Avatar;
            user.EmailConfirmedDate = dto.EmailConfirmedDate;
            user.InvitedDate = dto.InvitedDate;
            user.Kind = (UserKind)dto.Kind;

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
            Key = entity.Key,
            Disabled = entity.IsApproved == false,
            Email = entity.Email,
            Login = entity.Username,
            NoConsole = entity.IsLockedOut,
            Password = entity.RawPasswordValue,
            PasswordConfig = entity.PasswordConfiguration,
            UserLanguage = entity.Language,
            UserName = entity.Name!,
            SecurityStampToken = entity.SecurityStamp,
            FailedLoginAttempts = entity.FailedPasswordAttempts,
            LastLockoutDate = entity.LastLockoutDate == DateTime.MinValue ? null : entity.LastLockoutDate,
            LastLoginDate = entity.LastLoginDate == DateTime.MinValue ? null : entity.LastLoginDate,
            LastPasswordChangeDate = entity.LastPasswordChangeDate == DateTime.MinValue ? null : entity.LastPasswordChangeDate,
            CreateDate = entity.CreateDate,
            UpdateDate = entity.UpdateDate,
            Avatar = entity.Avatar,
            EmailConfirmedDate = entity.EmailConfirmedDate,
            InvitedDate = entity.InvitedDate,
            Kind = (short)entity.Kind
        };

        if (entity.StartContentIds is not null)
        {
            foreach (var startNodeId in entity.StartContentIds)
            {
                dto.UserStartNodeDtos.Add(new UserStartNodeDto
                {
                    StartNode = startNodeId,
                    StartNodeType = (int)UserStartNodeDto.StartNodeTypeValue.Content,
                    UserId = entity.Id,
                });
            }
        }

        if (entity.StartMediaIds is not null)
        {
            foreach (var startNodeId in entity.StartMediaIds)
            {
                dto.UserStartNodeDtos.Add(new UserStartNodeDto
                {
                    StartNode = startNodeId,
                    StartNodeType = (int)UserStartNodeDto.StartNodeTypeValue.Media,
                    UserId = entity.Id,
                });
            }
        }

        if (entity.HasIdentity)
        {
            dto.Id = entity.Id.SafeCast<int>();
        }

        return dto;
    }

    private static IReadOnlyUserGroup ToReadOnlyGroup(UserGroupDto group, IDictionary<string, IPermissionMapper> permissionMappers)
    {
        return new ReadOnlyUserGroup(
            group.Id,
            group.Key,
            group.Name,
            group.Icon,
            group.StartContentId,
            group.StartMediaId,
            group.Alias,
            group.UserGroup2LanguageDtos.Select(x => x.LanguageId),
            group.UserGroup2AppDtos.Select(x => x.AppAlias).WhereNotNull().ToArray(),
            group.UserGroup2PermissionDtos.Select(x => x.Permission).ToHashSet(),
            new HashSet<IGranularPermission>(group.UserGroup2GranularPermissionDtos.Select(granularPermission =>
            {
                if (permissionMappers.TryGetValue(granularPermission.Context, out IPermissionMapper? mapper))
                {
                    return mapper.MapFromDto(granularPermission);
                }

                return new UnknownTypeGranularPermission()
                {
                    Permission = granularPermission.Permission,
                    Context = granularPermission.Context
                };
            })),
            group.HasAccessToAllLanguages);
    }
}
