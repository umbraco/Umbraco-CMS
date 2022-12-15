namespace Umbraco.Cms.Api.Management.ViewModels.RedirectUrlManagement;

public class RedirectUrlViewModel
{
    public Guid Key { get; set; }

    public required string OriginalUrl { get; set; }

    public required string DestinationUrl { get; set; }

    public DateTime Created { get; set; }

    public Guid ContentKey { get; set; }

    public string? Culture { get; set; }
}
