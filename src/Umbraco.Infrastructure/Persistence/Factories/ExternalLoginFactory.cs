using Umbraco.Cms.Core.Extensions;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Factories;

internal static class ExternalLoginFactory
{
    /// <summary>
    /// Creates an <see cref="IIdentityUserToken"/> entity from the specified <see cref="ExternalLoginTokenDto"/>.
    /// Maps relevant properties from the DTO to the entity, including login provider, name, value, user or member key, and creation date.
    /// After construction, resets the entity's dirty properties to ensure a clean state.
    /// </summary>
    /// <param name="dto">The data transfer object containing external login token information.</param>
    /// <returns>
    /// An <see cref="IIdentityUserToken"/> entity populated with data from the provided DTO.
    /// </returns>
    public static IIdentityUserToken BuildEntity(ExternalLoginTokenDto dto)
    {
        var entity = new IdentityUserToken(dto.Id, dto.ExternalLoginDto.LoginProvider, dto.Name, dto.Value, dto.ExternalLoginDto.UserOrMemberKey.ToString(), dto.CreateDate.EnsureUtc());

        // reset dirty initial properties (U4-1946)
        entity.ResetDirtyProperties(false);
        return entity;
    }

    public static IIdentityUserLogin BuildEntity(ExternalLoginDto dto)
    {
        // If there exists a UserId - this means the database is still not migrated. E.g on the upgrade state.
        // At this point we have to manually set the key, to ensure external logins can be used to upgrade
        var key = dto.UserId.HasValue ? dto.UserId.Value.ToGuid().ToString() : dto.UserOrMemberKey.ToString();

        var entity =
            new IdentityUserLogin(dto.Id, dto.LoginProvider, dto.ProviderKey, key, dto.CreateDate.EnsureUtc())
            {
                UserData = dto.UserData,
            };

        // reset dirty initial properties (U4-1946)
        entity.ResetDirtyProperties(false);
        return entity;
    }

    /// <summary>
    /// Builds an <see cref="ExternalLoginDto"/> from the given <see cref="IIdentityUserLogin"/> entity.
    /// </summary>
    /// <param name="entity">The identity user login entity to convert.</param>
    /// <returns>An <see cref="ExternalLoginDto"/> representing the entity.</returns>
    public static ExternalLoginDto BuildDto(IIdentityUserLogin entity)
    {
        var dto = new ExternalLoginDto
        {
            Id = entity.Id,
            CreateDate = entity.CreateDate,
            LoginProvider = entity.LoginProvider,
            ProviderKey = entity.ProviderKey,
            UserOrMemberKey = entity.Key,
            UserData = entity.UserData,
        };

        return dto;
    }

    /// <summary>
    /// Builds an <see cref="ExternalLoginDto"/> from the specified <see cref="IExternalLogin"/> entity and user or member key.
    /// </summary>
    /// <param name="userOrMemberKey">The unique identifier for the user or member associated with the external login.</param>
    /// <param name="entity">The external login entity to convert.</param>
    /// <param name="id">An optional identifier for the external login. If not specified, a default value may be used.</param>
    /// <returns>An <see cref="ExternalLoginDto"/> representing the external login entity.</returns>
    public static ExternalLoginDto BuildDto(Guid userOrMemberKey, IExternalLogin entity, int? id = null)
    {
        var dto = new ExternalLoginDto
        {
            Id = id ?? default,
            UserOrMemberKey = userOrMemberKey,
            LoginProvider = entity.LoginProvider,
            ProviderKey = entity.ProviderKey,
            UserData = entity.UserData,
            CreateDate = DateTime.UtcNow,
        };

        return dto;
    }

    public static ExternalLoginTokenDto BuildDto(int externalLoginId, IExternalLoginToken token, int? id = null)
    {
        var dto = new ExternalLoginTokenDto
        {
            Id = id ?? default,
            ExternalLoginId = externalLoginId,
            Name = token.Name,
            Value = token.Value,
            CreateDate = DateTime.UtcNow,
        };

        return dto;
    }
}
