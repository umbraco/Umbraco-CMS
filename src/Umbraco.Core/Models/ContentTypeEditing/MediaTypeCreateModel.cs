namespace Umbraco.Cms.Core.Models.ContentTypeEditing;

public class MediaTypeCreateModel : MediaTypeModelBase
{
    public Guid? Key { get; set; }

    public Guid? ParentKey { get; set; }
}
