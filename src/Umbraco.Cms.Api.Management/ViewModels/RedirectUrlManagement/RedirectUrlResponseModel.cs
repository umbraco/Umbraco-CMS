namespace Umbraco.Cms.Api.Management.ViewModels.RedirectUrlManagement;

public class RedirectUrlResponseModel
{
    public Guid Id { get; set; }

    public required string OriginalUrl { get; set; }

    public required string DestinationUrl { get; set; }

    public DateTimeOffset Created { get; set; }

    public required ReferenceByIdModel Document { get; set; }

    public string? Culture { get; set; }
}
