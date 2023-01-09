using Umbraco.Cms.Core.Configuration;

namespace Umbraco.Cms.ManagementApi.ViewModels.ModelsBuilderDashboard;

public class ModelsBuilderViewModel
{
    public ModelsMode Mode { get; set; }

    public bool CanGenerate { get; set; }

    public bool OutOfDateModels { get; set; }

    public string? LastError { get; set; }

    public string? Version { get; set; }

    public string? ModelsNamespace { get; set; }

    public bool TrackingOutOfDateModels { get; set; }
}
