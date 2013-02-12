using System.Globalization;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;

namespace Umbraco.Core.Persistence.Factories
{
    internal class ServerRegistrationFactory : IEntityFactory<ServerRegistration, ServerRegistrationDto>
    {
        #region Implementation of IEntityFactory<Language,LanguageDto>

        public ServerRegistration BuildEntity(ServerRegistrationDto dto)
        {            
            return new ServerRegistration(dto.Id, dto.Address, dto.DateRegistered, dto.LastNotified);
        }

        public ServerRegistrationDto BuildDto(ServerRegistration entity)
        {
            var dto = new ServerRegistrationDto()
                {
                    Address = entity.ServerAddress,
                    DateRegistered = entity.CreateDate,
                    IsActive = entity.IsActive,
                    LastNotified = entity.UpdateDate
                };
            if (entity.HasIdentity)
                dto.Id = short.Parse(entity.Id.ToString(CultureInfo.InvariantCulture));

            return dto;
        }

        #endregion
    }
}