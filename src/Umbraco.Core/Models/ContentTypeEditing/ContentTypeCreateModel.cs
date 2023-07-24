namespace Umbraco.Cms.Core.Models.ContentTypeEditing;

public class ContentTypeCreateModel : ContentTypeModelBase
{
    /// <summary>
    /// We need to support specifying a key when creating a document type. But we do not want to mandate it.
    /// </summary>
    public Guid? Key { get; set; }

    /// <summary>
    /// The key of the parent container (folder) if any. NOT to be mistaken with content type inheritance.
    /// </summary>
    public Guid? ParentKey { get; set; }
}
