namespace Umbraco.Cms.Api.Management.ViewModels.DocumentType.Composition;

public class AvailableContentTypeCompositionResponseModel : ContentTypeCompositionModelBase
{
    /// <remarks>Empty when located at root.</remarks>
    public IEnumerable<string> FolderPath { get; set; } = Array.Empty<string>();

    public bool IsAllowed { get; set; }
}
