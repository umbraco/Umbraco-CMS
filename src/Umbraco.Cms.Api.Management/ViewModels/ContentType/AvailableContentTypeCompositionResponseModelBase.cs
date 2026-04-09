namespace Umbraco.Cms.Api.Management.ViewModels.ContentType;

/// <summary>
/// Base response model for an available content type composition.
/// </summary>
public abstract class AvailableContentTypeCompositionResponseModelBase : ContentTypeCompositionResponseModelBase
{
    /// <remarks>Empty when located at root.</remarks>
    public IEnumerable<string> FolderPath { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets a value indicating whether this content type composition is compatible with the current content type.
    /// </summary>
    public bool IsCompatible { get; set; }
}
