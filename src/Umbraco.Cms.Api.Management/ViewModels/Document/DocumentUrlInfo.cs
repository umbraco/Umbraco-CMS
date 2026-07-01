using Umbraco.Cms.Api.Management.ViewModels.Content;

namespace Umbraco.Cms.Api.Management.ViewModels.Document;

/// <summary>
/// Represents information about a document's URL, including its address and status.
/// </summary>
public class DocumentUrlInfo : ContentUrlInfoBase
{
    /// <summary>
    /// Gets or sets an optional message providing additional information about the document URL, such as errors or warnings.
    /// </summary>
    public required string? Message { get; init; }

    /// <summary>
    /// Gets or sets the name of the provider responsible for generating the document URL.
    /// </summary>
    public required string Provider { get; init; }
}
