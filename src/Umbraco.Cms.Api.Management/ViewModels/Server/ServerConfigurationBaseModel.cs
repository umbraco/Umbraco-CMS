namespace Umbraco.Cms.Api.Management.ViewModels.Server;

public class ServerConfigurationBaseModel
{
    public IDictionary<string, object> Items { get; set; } = new Dictionary<string, object>();
}
