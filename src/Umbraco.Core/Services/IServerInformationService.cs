using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Provides server information about the current Umbraco instance.
/// </summary>
public interface IServerInformationService
{
    /// <summary>
    ///     Gets information about the server including version, timezone, and runtime mode.
    /// </summary>
    /// <returns>A <see cref="ServerInformation"/> object containing the server details.</returns>
    ServerInformation GetServerInformation();
}
