namespace Umbraco.Cms.Core.Models.ContentEditing;

/// <summary>
///     Represents a model for updating domains associated with content.
/// </summary>
public class DomainsUpdateModel
{
    /// <summary>
    ///     Gets or sets the optional default ISO culture code for the domains.
    /// </summary>
    public string? DefaultIsoCode { get; set; }

    /// <summary>
    ///     Gets or sets the collection of domain models to update.
    /// </summary>
    public required IEnumerable<DomainModel> Domains { get; set; }
}
