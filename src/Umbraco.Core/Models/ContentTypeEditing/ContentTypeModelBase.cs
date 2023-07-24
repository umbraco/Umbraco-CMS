namespace Umbraco.Cms.Core.Models.ContentTypeEditing;

public class ContentTypeModelBase : ContentTypeEditingModelBase<ContentTypePropertyTypeModel, ContentTypePropertyContainerModel>
{
    public bool IsElement { get; set; }

    public ContentTypeCleanup Cleanup { get; set; } = new();

    public IEnumerable<Guid> AllowedTemplateKeys { get; set; } = Array.Empty<Guid>();

    public Guid? DefaultTemplateKey { get; set; }
}
