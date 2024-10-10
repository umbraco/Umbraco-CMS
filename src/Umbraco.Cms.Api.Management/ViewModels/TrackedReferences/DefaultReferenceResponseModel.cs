namespace Umbraco.Cms.Api.Management.ViewModels.TrackedReferences;

public class DefaultReferenceResponseModel : IReferenceResponseModel
{
    public Guid Id { get; set; }

    public string? Name { get; set; }

    public string? Type { get; set; }

    public string? Icon { get; set; }
}
