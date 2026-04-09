using Umbraco.Cms.Core.Configuration.Models;

namespace Umbraco.Cms.Api.Management.ViewModels.Server;

/// <summary>
/// Represents a model for the response containing detailed information about the server.
/// </summary>
public class ServerInformationResponseModel
{
    /// <summary>
    /// Gets or sets the version string of the Umbraco server instance.
    /// </summary>
    public string Version { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the version of the server's assembly.
    /// </summary>
    public string AssemblyVersion { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the base UTC offset of the server.
    /// </summary>
    public string BaseUtcOffset { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the current runtime mode of the Umbraco server.
    /// </summary>
    public RuntimeMode RuntimeMode { get; set; }
}
