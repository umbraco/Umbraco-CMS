namespace Umbraco.Cms.Core.Models.ContentTypeEditing.Document;

public class DocumentTypeBase : ContentTypeBase<DocumentPropertyType, DocumentTypePropertyContainer>
{
    public bool IsElement { get; set; }
}
