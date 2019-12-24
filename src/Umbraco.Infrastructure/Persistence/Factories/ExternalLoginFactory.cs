using Umbraco.Core.Models.Identity;
using Umbraco.Core.Persistence.Dtos;

namespace Umbraco.Core.Persistence.Factories
{
    internal static class ExternalLoginFactory
    {
        public static IIdentityUserLogin BuildEntity(ExternalLoginDto dto)
        {
            var entity = new IdentityUserLogin(dto.Id, dto.LoginProvider, dto.ProviderKey, dto.UserId, dto.CreateDate);

            // reset dirty initial properties (U4-1946)
            entity.ResetDirtyProperties(false);
            return entity;
        }

        public static ExternalLoginDto BuildDto(IIdentityUserLogin entity)
        {
            var dto = new ExternalLoginDto
            {
                Id = entity.Id,
                CreateDate = entity.CreateDate,
                LoginProvider = entity.LoginProvider,
                ProviderKey = entity.ProviderKey,
                UserId = entity.UserId
            };

            return dto;
        }
    }
}
