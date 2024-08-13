namespace Umbraco.Cms.Core.Models.ContentTypeEditing;

public class ContentTypeCreateModel : ContentTypeModelBase
{
    public Guid? Key { get; set; }

    public Guid? ContainerKey { get; set; }
}
