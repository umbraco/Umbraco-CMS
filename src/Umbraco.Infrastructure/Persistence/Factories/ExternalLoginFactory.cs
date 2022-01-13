using System;
using System.Globalization;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Persistence.Factories
{
    internal static class ExternalLoginFactory
    {
        public static IIdentityUserToken BuildEntity(ExternalLoginTokenDto dto)
        {
            var entity = new IdentityUserToken(dto.Id, dto.ExternalLoginDto.LoginProvider, dto.Name, dto.Value, dto.ExternalLoginDto.UserOrMemberKey.ToString(), dto.CreateDate);

            // reset dirty initial properties (U4-1946)
            entity.ResetDirtyProperties(false);
            return entity;
        }

        public static IIdentityUserLogin BuildEntity(ExternalLoginDto dto)
        {
            var entity = new IdentityUserLogin(dto.Id, dto.LoginProvider, dto.ProviderKey, dto.UserOrMemberKey.ToString(), dto.CreateDate)
            {
                UserData = dto.UserData
            };

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
                UserOrMemberKey = entity.Key,
                UserData = entity.UserData
            };

            return dto;
        }

        public static ExternalLoginDto BuildDto(Guid userOrMemberKey, IExternalLogin entity, int? id = null)
        {
            var dto = new ExternalLoginDto
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

        public static ExternalLoginTokenDto BuildDto(int externalLoginId, IExternalLoginToken token, int? id = null)
        {
            var dto = new ExternalLoginTokenDto
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
