namespace Umbraco.Cms.Core.Services;

public interface ISystemInformationService
{
    /// <summary>
    /// Retrieves various system/server information (i.e. OS and framework version) and Umbraco configuration (i.e. runtime mode and server role).
    /// </summary>
    /// <returns>Key/value pairs of system information</returns>
    IDictionary<string, string> GetTroubleshootingInformation();

    /// <summary>
    /// Retrieves various system/server information (i.e. OS and assembly version) and Umbraco configuration (i.e. runtime mode and serverTime offset).
    /// </summary>
    /// <returns>Key/value pairs of system information</returns>
    IDictionary<string, object> GetServerInformation();
}
