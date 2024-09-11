using Umbraco.Cms.Core.Configuration.Models;

namespace Umbraco.Cms.Api.Management.ViewModels.Server;

public class ServerInformationResponseModel
{
    public string Version { get; set; } = string.Empty;
    public string AssemblyVersion { get; set; } = string.Empty;
    public string BaseUtcOffset { get; set; } = string.Empty;
    public RuntimeMode RuntimeMode { get; set; }
}
