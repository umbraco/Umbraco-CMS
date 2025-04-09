namespace Umbraco.Cms.Api.Management.ViewModels.TrackedReferences;

public abstract class ReferenceResponseModel : IReferenceResponseModel
{
    public Guid Id { get; set; }

    public string? Name { get; set; }
}
