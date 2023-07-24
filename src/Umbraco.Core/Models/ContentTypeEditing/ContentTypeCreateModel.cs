namespace Umbraco.Cms.Core.Models.ContentTypeEditing;

public class ContentTypeCreateModel : ContentTypeModelBase
{
    // We need to support specifying a key when creating a document type. But we do not want to mandate it.
    public Guid? Key { get; set; }
}
