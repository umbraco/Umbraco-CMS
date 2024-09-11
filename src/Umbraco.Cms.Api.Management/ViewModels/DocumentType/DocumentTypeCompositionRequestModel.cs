using Umbraco.Cms.Api.Management.ViewModels.ContentType;

namespace Umbraco.Cms.Api.Management.ViewModels.DocumentType;

public class DocumentTypeCompositionRequestModel : ContentTypeCompositionRequestModelBase
{
    /// <summary>
    ///     Gets or sets a value indicating whether the content type is currently marked as an element type.
    /// </summary>
    public bool IsElement { get; set; }
}
