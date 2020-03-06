using System.Configuration;
using Umbraco.Core.Configuration;

namespace Umbraco.Configuration
{
    public class ActiveDirectorySettings : IActiveDirectorySettings
    {
        public ActiveDirectorySettings()
        {
            ActiveDirectoryDomain = ConfigurationManager.AppSettings["ActiveDirectoryDomain"];
        }

        public string ActiveDirectoryDomain { get; }
    }
}
