namespace Umbraco.Cms.Api.Management.ViewModels.ContentType;

public abstract class ContentTypeReferenceResponseModelBase
{
    public Guid Id { get; set; }

    public string Icon { get; set; } = string.Empty;

    // FIXME: ensure that this naming is consistent with the "full" content type model representation for listviews (pending listview implementation)
    public bool HasListView { get; set; }
}
