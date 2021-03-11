using System;
using Umbraco.Cms.Core.Models.Identity;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Persistence.Factories
{
    internal static class ExternalLoginFactory
    {
        public static IIdentityUserToken BuildEntity(ExternalLoginTokenDto dto)
        {
            var entity = new IdentityUserToken(dto.Id, dto.ExternalLoginDto.LoginProvider, dto.Name, dto.Value, dto.ExternalLoginDto.UserId.ToString(), dto.CreateDate);

            // reset dirty initial properties (U4-1946)
            entity.ResetDirtyProperties(false);
            return entity;
        }

        public static IIdentityUserLogin BuildEntity(ExternalLoginDto dto)
        {
            var entity = new IdentityUserLogin(dto.Id, dto.LoginProvider, dto.ProviderKey, dto.UserId.ToString(), dto.CreateDate)
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
                UserId = int.Parse(entity.UserId), // TODO: This is temp until we change the ext logins to use GUIDs
                UserData = entity.UserData
            };

            return dto;
        }

        public static ExternalLoginDto BuildDto(int userId, IExternalLogin entity, int? id = null)
        {
            var dto = new ExternalLoginDto
            {
                Id = id ?? default,
                UserId = userId,
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
