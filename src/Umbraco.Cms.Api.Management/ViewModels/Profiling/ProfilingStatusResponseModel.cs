namespace Umbraco.Cms.Api.Management.ViewModels.Profiling;

/// <summary>
/// Represents a response model containing information about the current profiling status in the system.
/// </summary>
public class ProfilingStatusResponseModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ProfilingStatusResponseModel"/> class with the specified profiling status.
    /// </summary>
    /// <param name="enabled">True if profiling is enabled; otherwise, false.</param>
    public ProfilingStatusResponseModel(bool enabled) => Enabled = enabled;

    /// <summary>
    /// Indicates whether profiling is enabled.
    /// </summary>
    public bool Enabled { get; }
}
