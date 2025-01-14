namespace Umbraco.Cms.Api.Management.ViewModels.Profiling;

public class ProfilingStatusRequestModel
{
    public ProfilingStatusRequestModel(bool enabled) => Enabled = enabled;

    public bool Enabled { get; }
}
