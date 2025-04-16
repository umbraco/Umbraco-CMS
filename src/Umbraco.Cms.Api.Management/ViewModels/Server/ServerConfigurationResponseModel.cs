namespace Umbraco.Cms.Api.Management.ViewModels.Server;

public class ServerConfigurationResponseModel
{
    public bool AllowPasswordReset { get; set; }

    public int VersionCheckPeriod { get; set; }

    public bool AllowLocalLogin { get; set; }
}
