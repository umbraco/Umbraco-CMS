namespace Umbraco.Cms.Core.Models.ContentTypeEditing;

public class MediaTypeCreateModel : MediaTypeModelBase, IContentTypeCreate
{
    public Guid? Key { get; set; }

    public Guid? ParentKey { get; set; }
}
