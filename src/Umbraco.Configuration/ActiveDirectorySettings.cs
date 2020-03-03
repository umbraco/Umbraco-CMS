using System.Configuration;
using Umbraco.Core.Configuration;

namespace Umbraco.Configuration
{
    public class ActiveDirectory : IActiveDirectorySettings
    {
        public ActiveDirectory()
        {
            ActiveDirectoryDomain = ConfigurationManager.AppSettings["ActiveDirectoryDomain"];
        }

        public string ActiveDirectoryDomain { get; }
    }
}
