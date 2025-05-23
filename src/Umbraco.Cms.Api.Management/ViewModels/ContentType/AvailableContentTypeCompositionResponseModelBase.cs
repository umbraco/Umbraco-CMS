namespace Umbraco.Cms.Api.Management.ViewModels.ContentType;

public abstract class AvailableContentTypeCompositionResponseModelBase : ContentTypeCompositionResponseModelBase
{
    /// <remarks>Empty when located at root.</remarks>
    public IEnumerable<string> FolderPath { get; set; } = Array.Empty<string>();

    public bool IsCompatible { get; set; }
}
