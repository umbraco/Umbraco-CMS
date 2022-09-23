namespace Umbraco.Cms.ManagementApi.ViewModels.Profiling;

public class ProfilingStatusViewModel
{
    public ProfilingStatusViewModel(bool enabled) => Enabled = enabled;

    public bool Enabled { get; }
}
