namespace Umbraco.Cms.Core.Models.ContentTypeEditing.Document;

public class DocumentTypeModelBase : ContentTypeModelBase<DocumentPropertyTypeModel, DocumentTypePropertyContainerModel>
{
    public bool IsElement { get; set; }

    public ContentTypeCleanup Cleanup { get; set; } = new();

    public IEnumerable<Guid> AllowedTemplateKeys { get; set; } = Array.Empty<Guid>();

    public Guid? DefaultTemplateKey { get; set; }
}
