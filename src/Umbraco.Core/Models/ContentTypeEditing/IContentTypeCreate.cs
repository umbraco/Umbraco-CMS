namespace Umbraco.Cms.Core.Models.ContentTypeEditing;

public interface IContentTypeCreate
{
    /// <summary>
    /// We need to support specifying a key when creating a document type. But we do not want to mandate it.
    /// </summary>
    Guid? Key { get; set; }

    /// <summary>
    /// The key of the parent container (folder) if any. NOT to be mistaken with content type inheritance.
    /// </summary>
    Guid? ParentKey { get; set; }
}
