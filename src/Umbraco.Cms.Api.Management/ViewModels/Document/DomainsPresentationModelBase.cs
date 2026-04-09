namespace Umbraco.Cms.Api.Management.ViewModels.Document;

/// <summary>
/// Serves as the base view model for presenting domain-related information in documents.
/// </summary>
public abstract class DomainsPresentationModelBase
{
    /// <summary>
    /// Gets or sets the ISO code that is used as the default language for the domain.
    /// </summary>
    public string? DefaultIsoCode { get; set; }

    /// <summary>
    /// Gets or sets the collection of domains associated with the document.
    /// </summary>
    public required IEnumerable<DomainPresentationModel> Domains { get; set; }
}
