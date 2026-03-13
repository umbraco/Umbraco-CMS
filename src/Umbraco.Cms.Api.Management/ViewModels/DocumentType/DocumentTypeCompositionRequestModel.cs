using Umbraco.Cms.Api.Management.ViewModels.ContentType;

namespace Umbraco.Cms.Api.Management.ViewModels.DocumentType;

/// <summary>
/// Represents the data required to compose or associate document types in Umbraco via an API request.
/// </summary>
public class DocumentTypeCompositionRequestModel : ContentTypeCompositionRequestModelBase
{
    /// <summary>
    ///     Gets or sets a value indicating whether the content type is currently marked as an element type.
    /// </summary>
    public bool IsElement { get; set; }
}
