using System.Collections.Generic;
using Umbraco.Core.Services;

namespace Umbraco.Core.Sync
{
    //NOTE: SD: Commenting out for now until we want to release a distributed cache provider that 
    // uses internal DNS names for each website to 'call' home intead of the current configuration based approach.

    ///// <summary>
    ///// A registrar that stores registered server nodes in a database
    ///// </summary>
    //internal class DatabaseServerRegistrar : IServerRegistrar
    //{
    //    private readonly ServerRegistrationService _registrationService;

    //    public DatabaseServerRegistrar(ServerRegistrationService registrationService)
    //    {
    //        _registrationService = registrationService;
    //    }

    //    public IEnumerable<IServerAddress> Registrations
    //    {
    //        get { return _registrationService.GetActiveServers(); }
    //    }
    //}
}