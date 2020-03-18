using System;
using Microsoft.Extensions.Configuration;
using Umbraco.Core.Configuration;

namespace Umbraco.Configuration.Models
{
    public class MachineKeyConfig : IMachineKeyConfig
    {
        private readonly IConfiguration _configuration;

        public MachineKeyConfig(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        //TODO all the machineKey stuff should be replaced: https://docs.microsoft.com/en-us/aspnet/core/security/data-protection/compatibility/replacing-machinekey?view=aspnetcore-3.1

        public bool HasMachineKey => throw new NotImplementedException("TODO we need to figure out what to do here");
    }
}
