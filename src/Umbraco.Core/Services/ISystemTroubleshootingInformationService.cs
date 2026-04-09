using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services;

/// <summary>
/// Service for retrieving system troubleshooting information.
/// </summary>
public interface ISystemTroubleshootingInformationService
{
    /// <summary>
    /// Retrieves various system/server information (i.e. OS and framework version) and Umbraco configuration (i.e. runtime mode and server role).
    /// </summary>
    /// <returns>Key/value pairs of system information</returns>
    IDictionary<string, string> GetTroubleshootingInformation();
}

