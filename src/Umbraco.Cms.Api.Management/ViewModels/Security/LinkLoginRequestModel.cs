namespace Umbraco.Cms.Api.Management.ViewModels.Security;

public class LinkLoginRequestModel
{
    public required string Provider { get; set; }

    public required Guid LinkKey { get; set; }
}
