using Umbraco.Web.HealthCheck.Checks.Config;

namespace Umbraco.Core.Configuration.HealthChecks
{
    public interface IConfigurationService
    {
        ConfigurationServiceResult UpdateConfigFile(string value);
    }
}
