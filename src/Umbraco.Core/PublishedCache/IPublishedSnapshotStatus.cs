namespace Umbraco.Cms.Core.PublishedCache;

/// <summary>
///     Returns the currents status for nucache
/// </summary>
public interface IPublishedSnapshotStatus
{
    /// <summary>
    ///     Gets the URL used to retreive the status
    /// </summary>
    string StatusUrl { get; }

    /// <summary>
    ///     Gets the status report as a string
    /// </summary>
    string GetStatus();
}
