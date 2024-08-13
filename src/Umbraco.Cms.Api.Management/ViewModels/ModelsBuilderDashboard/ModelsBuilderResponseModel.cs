using Umbraco.Cms.Core.Configuration;

namespace Umbraco.Cms.Api.Management.ViewModels.ModelsBuilderDashboard;

public class ModelsBuilderResponseModel
{
    public ModelsMode Mode { get; set; }

    public bool CanGenerate { get; set; }

    public bool OutOfDateModels { get; set; }

    public string? LastError { get; set; }

    public string? Version { get; set; }

    public string? ModelsNamespace { get; set; }

    public bool TrackingOutOfDateModels { get; set; }
}
