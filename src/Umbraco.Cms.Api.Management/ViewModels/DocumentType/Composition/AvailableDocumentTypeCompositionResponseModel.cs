using Umbraco.Cms.Api.Management.ViewModels.ContentType;

namespace Umbraco.Cms.Api.Management.ViewModels.DocumentType.Composition;

public class AvailableDocumentTypeCompositionResponseModel : ContentTypeCompositionModelBase
{
    /// <remarks>Empty when located at root.</remarks>
    public IEnumerable<string> FolderPath { get; set; } = Array.Empty<string>();

    public bool IsCompatible { get; set; }
}
