namespace Umbraco.Cms.Api.Management.ViewModels.DocumentType.Composition;

public class ContentTypeCompositionModelBase
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Icon { get; set; } = string.Empty;
}
