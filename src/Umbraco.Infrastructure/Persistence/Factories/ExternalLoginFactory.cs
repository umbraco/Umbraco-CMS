using System;
using Umbraco.Core.Models.Identity;
using Umbraco.Core.Persistence.Dtos;

namespace Umbraco.Core.Persistence.Factories
{
    internal static class ExternalLoginFactory
    {
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
    }
}
