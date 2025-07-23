namespace Umbraco.Cms.Api.Management.ViewModels.TrackedReferences;

public abstract class TrackedReferenceContentType
{
    public Guid Id { get; set; }

    public string? Icon { get; set; }

    public string? Alias { get; set; }

    public string? Name { get; set; }
}
