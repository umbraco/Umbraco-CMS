using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Factories;

internal static class UserFactory
{
    public static IUser BuildEntity(GlobalSettings globalSettings, UserDto dto)
    {
        var guidId = dto.Id.ToGuid();

        var user = new User(globalSettings, dto.Id, dto.UserName, dto.Email, dto.Login, dto.Password,
            dto.PasswordConfig,
            dto.UserGroupDtos.Select(x => ToReadOnlyGroup(x)).ToArray(),
            dto.UserStartNodeDtos.Where(x => x.StartNodeType == (int)UserStartNodeDto.StartNodeTypeValue.Content)
                .Select(x => x.StartNode).ToArray(),
            dto.UserStartNodeDtos.Where(x => x.StartNodeType == (int)UserStartNodeDto.StartNodeTypeValue.Media)
                .Select(x => x.StartNode).ToArray());

        try
        {
            user.DisableChangeTracking();

            user.Key = guidId;
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
            user.TourData = dto.TourData;

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
            PasswordConfig = entity.PasswordConfiguration,
            UserLanguage = entity.Language,
            UserName = entity.Name!,
            SecurityStampToken = entity.SecurityStamp,
            FailedLoginAttempts = entity.FailedPasswordAttempts,
            LastLockoutDate = entity.LastLockoutDate == DateTime.MinValue ? null : entity.LastLockoutDate,
            LastLoginDate = entity.LastLoginDate == DateTime.MinValue ? null : entity.LastLoginDate,
            LastPasswordChangeDate =
                entity.LastPasswordChangeDate == DateTime.MinValue ? null : entity.LastPasswordChangeDate,
            CreateDate = entity.CreateDate,
            UpdateDate = entity.UpdateDate,
            Avatar = entity.Avatar,
            EmailConfirmedDate = entity.EmailConfirmedDate,
            InvitedDate = entity.InvitedDate,
            TourData = entity.TourData,
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

    private static IReadOnlyUserGroup ToReadOnlyGroup(UserGroupDto group) =>
        new ReadOnlyUserGroup(group.Id, group.Name, group.Icon,
            group.StartContentId, group.StartMediaId, group.Alias, group.UserGroup2LanguageDtos.Select(x => x.LanguageId),
            group.UserGroup2AppDtos.Select(x => x.AppAlias).WhereNotNull().ToArray(),
            group.DefaultPermissions == null
                ? Enumerable.Empty<string>()
                : group.DefaultPermissions.ToCharArray().Select(x => x.ToString()),
            group.HasAccessToAllLanguages);
}
