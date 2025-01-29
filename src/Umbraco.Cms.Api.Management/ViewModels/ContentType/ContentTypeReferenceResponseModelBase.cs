namespace Umbraco.Cms.Api.Management.ViewModels.ContentType;

public abstract class ContentTypeReferenceResponseModelBase
{
    public Guid Id { get; set; }

    public string Icon { get; set; } = string.Empty;

    public ReferenceByIdModel? Collection { get; set; }
}
