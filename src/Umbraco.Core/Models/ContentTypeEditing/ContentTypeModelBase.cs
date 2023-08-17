namespace Umbraco.Cms.Core.Models.ContentTypeEditing;

public abstract class ContentTypeModelBase : ContentTypeEditingModelBase<ContentTypePropertyTypeModel, ContentTypePropertyContainerModel>
{
    public ContentTypeCleanup Cleanup { get; set; } = new();

    public IEnumerable<Guid> AllowedTemplateKeys { get; set; } = Array.Empty<Guid>();

    public Guid? DefaultTemplateKey { get; set; }
}
