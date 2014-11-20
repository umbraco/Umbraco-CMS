using System.Globalization;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;

namespace Umbraco.Core.Persistence.Factories
{
    internal class ServerRegistrationFactory 
    {
        #region Implementation of IEntityFactory<Language,LanguageDto>

        public ServerRegistration BuildEntity(ServerRegistrationDto dto)
        {
            var model = new ServerRegistration(dto.Id, dto.Address, dto.ComputerName, dto.DateRegistered, dto.LastNotified, dto.IsActive);
            //on initial construction we don't want to have dirty properties tracked
            // http://issues.umbraco.org/issue/U4-1946
            model.ResetDirtyProperties(false);
            return model;
        }

        public ServerRegistrationDto BuildDto(ServerRegistration entity)
        {
            var dto = new ServerRegistrationDto()
                {
                    Address = entity.ServerAddress,
                    DateRegistered = entity.CreateDate,
                    IsActive = entity.IsActive,
                    LastNotified = entity.UpdateDate,
                    ComputerName = entity.ComputerName
                };
            if (entity.HasIdentity)
                dto.Id = int.Parse(entity.Id.ToString(CultureInfo.InvariantCulture));

            return dto;
        }

        #endregion
    }
}