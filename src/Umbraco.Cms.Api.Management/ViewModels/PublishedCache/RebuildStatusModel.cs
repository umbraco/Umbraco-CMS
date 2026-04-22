namespace Umbraco.Cms.Api.Management.ViewModels.PublishedCache;

/// <summary>
/// Represents the status of a published cache rebuild operation.
/// </summary>
public class RebuildStatusModel
{
    /// <summary>
    /// Gets or sets a value indicating whether the rebuild process is currently running.
    /// </summary>
    public bool IsRebuilding { get; set; }
}
