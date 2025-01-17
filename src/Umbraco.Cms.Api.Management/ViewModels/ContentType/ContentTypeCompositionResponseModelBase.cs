namespace Umbraco.Cms.Api.Management.ViewModels.ContentType;

public abstract class ContentTypeCompositionResponseModelBase
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Icon { get; set; } = string.Empty;
}
