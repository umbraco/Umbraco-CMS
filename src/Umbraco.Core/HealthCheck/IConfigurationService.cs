namespace Umbraco.Core.HealthCheck
{
    public interface IConfigurationService
    {
        ConfigurationServiceResult UpdateConfigFile(string value, string itemPath);
    }
}
