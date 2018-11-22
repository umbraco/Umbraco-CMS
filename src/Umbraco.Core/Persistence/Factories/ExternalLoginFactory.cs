using Umbraco.Core.Models.Identity;
using Umbraco.Core.Models.Rdbms;

namespace Umbraco.Core.Persistence.Factories
{
    internal class ExternalLoginFactory
    {
        public IIdentityUserLogin BuildEntity(ExternalLoginDto dto)
        {
            var entity = new IdentityUserLogin(dto.Id, dto.LoginProvider, dto.ProviderKey, dto.UserId, dto.CreateDate);

            //on initial construction we don't want to have dirty properties tracked
            // http://issues.umbraco.org/issue/U4-1946
            entity.ResetDirtyProperties(false);
            return entity;
        }

        public ExternalLoginDto BuildDto(IIdentityUserLogin entity)
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