namespace Umbraco.Cms.Api.Management.ViewModels.Profiling;

/// <summary>
/// Serves as the base class for view models that present profiling status information.
/// </summary>
public class ProfilingStatusPresentationBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ProfilingStatusPresentationBase"/> class with the specified profiling status.
    /// </summary>
    /// <param name="enabled">Indicates whether profiling is enabled.</param>
    public ProfilingStatusPresentationBase(bool enabled) => Enabled = enabled;

    /// <summary>
    /// Gets a value indicating whether profiling is enabled.
    /// </summary>
    public bool Enabled { get; }
}
