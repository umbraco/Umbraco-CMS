using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services;

/// <summary>
/// Service for retrieving usage information about the Umbraco installation.
/// </summary>
public interface IUsageInformationService
{
    /// <summary>
    /// Gets detailed usage information about the Umbraco installation.
    /// </summary>
    /// <returns>A collection of <see cref="UsageInformation"/> objects, or <c>null</c> if unavailable.</returns>
    IEnumerable<UsageInformation>? GetDetailed();
}
