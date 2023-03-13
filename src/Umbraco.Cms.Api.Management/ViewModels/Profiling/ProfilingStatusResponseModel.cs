namespace Umbraco.Cms.Api.Management.ViewModels.Profiling;

public class ProfilingStatusResponseModel
{
    public ProfilingStatusResponseModel(bool enabled) => Enabled = enabled;

    public bool Enabled { get; }
}
