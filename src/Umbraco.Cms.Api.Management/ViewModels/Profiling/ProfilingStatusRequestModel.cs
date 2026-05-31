namespace Umbraco.Cms.Api.Management.ViewModels.Profiling;

/// <summary>
/// Represents the request model used to query or update the profiling status in the management API.
/// </summary>
public class ProfilingStatusRequestModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ProfilingStatusRequestModel"/> class.
    /// </summary>
    /// <param name="enabled">True to enable profiling; otherwise, false.</param>
    public ProfilingStatusRequestModel(bool enabled) => Enabled = enabled;

    /// <summary>
    /// Gets a value indicating whether profiling is enabled.
    /// </summary>
    public bool Enabled { get; }
}
