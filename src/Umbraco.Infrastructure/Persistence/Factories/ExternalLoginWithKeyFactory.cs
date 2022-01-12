using System;
using System.Globalization;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Persistence.Factories
{
    internal static class ExternalLoginWithKeyFactory
    {
        public static IIdentityUserToken BuildEntity(ExternalLoginTokenWithKeyDto dto)
        {
            var entity = new IdentityUserToken(dto.Id, dto.ExternalLoginDto.LoginProvider, dto.Name, dto.Value, dto.ExternalLoginDto.UserOrMemberKey.ToString(), dto.CreateDate);

            // reset dirty initial properties (U4-1946)
            entity.ResetDirtyProperties(false);
            return entity;
        }

        public static IIdentityUserLogin BuildEntity(ExternalLoginWithKeyDto dto)
        {
            var entity = new IdentityUserLogin(dto.Id, dto.LoginProvider, dto.ProviderKey, dto.UserOrMemberKey.ToString(), dto.CreateDate)
            {
                UserData = dto.UserData
            };

            // reset dirty initial properties (U4-1946)
            entity.ResetDirtyProperties(false);
            return entity;
        }

        public static ExternalLoginWithKeyDto BuildDto(IIdentityUserLogin entity)
        {
            var dto = new ExternalLoginWithKeyDto
            {
                Id = entity.Id,
                CreateDate = entity.CreateDate,
                LoginProvider = entity.LoginProvider,
                ProviderKey = entity.ProviderKey,
                UserOrMemberKey = entity.Key,
                UserData = entity.UserData
            };

            return dto;
        }

        public static ExternalLoginWithKeyDto BuildDto(Guid userOrMemberKey, IExternalLogin entity, int? id = null)
        {
            var dto = new ExternalLoginWithKeyDto
            {
                Id = id ?? default,
                UserOrMemberKey = userOrMemberKey,
                LoginProvider = entity.LoginProvider,
                ProviderKey = entity.ProviderKey,
                UserData = entity.UserData,
                CreateDate = DateTime.Now
            };

            return dto;
        }

        public static ExternalLoginTokenWithKeyDto BuildDto(int externalLoginId, IExternalLoginToken token, int? id = null)
        {
            var dto = new ExternalLoginTokenWithKeyDto
            {
                Id = id ?? default,
                ExternalLoginId = externalLoginId,
                Name = token.Name,
                Value = token.Value,
                CreateDate = DateTime.Now
            };

            return dto;
        }
    }
}
