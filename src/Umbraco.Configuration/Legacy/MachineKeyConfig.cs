using System.Configuration;
using Umbraco.Core.Configuration;

namespace Umbraco.Configuration.Legacy
{
    public class MachineKeyConfig : IMachineKeyConfig
    {
        //TODO all the machineKey stuff should be replaced: https://docs.microsoft.com/en-us/aspnet/core/security/data-protection/compatibility/replacing-machinekey?view=aspnetcore-3.1

        public bool HasMachineKey
        {
            get
            {
                var machineKeySection =
                    ConfigurationManager.GetSection("system.web/machineKey") as ConfigurationSection;
                return !(machineKeySection?.ElementInformation?.Source is null);
            }
        }
    }
}
