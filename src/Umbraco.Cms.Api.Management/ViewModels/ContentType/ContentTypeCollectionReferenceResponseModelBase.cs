namespace Umbraco.Cms.Api.Management.ViewModels.ContentType;

public abstract class ContentTypeCollectionReferenceResponseModelBase
{
    public Guid Id { get; set; }

    public string Alias { get; set; } = string.Empty;

    public string Icon { get; set; } = string.Empty;
}
