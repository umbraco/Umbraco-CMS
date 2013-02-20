using System.Globalization;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;

namespace Umbraco.Core.Persistence.Factories
{
    //NOTE: SD: Commenting out for now until we want to release a distributed cache provider that 
    // uses internal DNS names for each website to 'call' home intead of the current configuration based approach.

    //internal class ServerRegistrationFactory : IEntityFactory<ServerRegistration, ServerRegistrationDto>
    //{
    //    #region Implementation of IEntityFactory<Language,LanguageDto>

    //    public ServerRegistration BuildEntity(ServerRegistrationDto dto)
    //    {
    //        return new ServerRegistration(dto.Id, dto.Address, dto.ComputerName, dto.DateRegistered, dto.LastNotified);
    //    }

    //    public ServerRegistrationDto BuildDto(ServerRegistration entity)
    //    {
    //        var dto = new ServerRegistrationDto()
    //            {
    //                Address = entity.ServerAddress,
    //                DateRegistered = entity.CreateDate,
    //                IsActive = entity.IsActive,
    //                LastNotified = entity.UpdateDate,
    //                ComputerName = entity.ComputerName
    //            };
    //        if (entity.HasIdentity)
    //            dto.Id = short.Parse(entity.Id.ToString(CultureInfo.InvariantCulture));

    //        return dto;
    //    }

    //    #endregion
    //}
}