namespace Umbraco.Cms.Api.Management.ViewModels.Profiling;

public class ProfilingStatusPresentationBase
{
    public ProfilingStatusPresentationBase(bool enabled) => Enabled = enabled;

    public bool Enabled { get; }
}
