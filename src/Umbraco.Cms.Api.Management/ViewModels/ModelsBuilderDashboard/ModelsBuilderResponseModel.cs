using Umbraco.Cms.Core.Configuration;

namespace Umbraco.Cms.Api.Management.ViewModels.ModelsBuilderDashboard;

/// <summary>
/// Represents a response returned by the Models Builder dashboard in the Umbraco CMS Management API.
/// </summary>
public class ModelsBuilderResponseModel
{
    /// <summary>
    /// Gets or sets the current operational mode of the ModelsBuilder, indicating how models are generated (e.g., PureLive, Dll, etc.).
    /// </summary>
    public required string Mode { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether model generation is possible.
    /// </summary>
    public bool CanGenerate { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the models are out of date.
    /// </summary>
    public bool OutOfDateModels { get; set; }

    /// <summary>Gets or sets the last error message encountered by the ModelsBuilder.</summary>
    public string? LastError { get; set; }

    /// <summary>
    /// Gets or sets the version number of the ModelsBuilder used in the system.
    /// </summary>
    public string? Version { get; set; }

    /// <summary>
    /// Gets or sets the namespace used for the generated models.
    /// </summary>
    public string? ModelsNamespace { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the models builder is tracking out-of-date models.
    /// </summary>
    public bool TrackingOutOfDateModels { get; set; }
}
