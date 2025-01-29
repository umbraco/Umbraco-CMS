namespace Umbraco.Cms.Core.Models.ContentEditing;

public abstract class ContentCreationModelBase : ContentEditingModelBase
{
    public Guid? Key { get; set; }

    public Guid ContentTypeKey { get; set; } = Guid.Empty;

    public Guid? ParentKey { get; set; }
}
