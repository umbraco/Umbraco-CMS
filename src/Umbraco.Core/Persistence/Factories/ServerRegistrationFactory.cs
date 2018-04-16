using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Dtos;

namespace Umbraco.Core.Persistence.Factories
{
    internal class ServerRegistrationFactory
    {
        public ServerRegistration BuildEntity(ServerRegistrationDto dto)
        {
            var model = new ServerRegistration(dto.Id, dto.ServerAddress, dto.ServerIdentity, dto.DateRegistered, dto.DateAccessed, dto.IsActive, dto.IsMaster);
            // reset dirty initial properties (U4-1946)
            model.ResetDirtyProperties(false);
            return model;
        }

        public ServerRegistrationDto BuildDto(IServerRegistration entity)
        {
            var dto = new ServerRegistrationDto
            {
                ServerAddress = entity.ServerAddress,
                DateRegistered = entity.CreateDate,
                IsActive = entity.IsActive,
                IsMaster = ((ServerRegistration) entity).IsMaster,
                DateAccessed = entity.UpdateDate,
                ServerIdentity = entity.ServerIdentity
            };

            if (entity.HasIdentity)
                dto.Id = entity.Id;

            return dto;
        }
    }
}
