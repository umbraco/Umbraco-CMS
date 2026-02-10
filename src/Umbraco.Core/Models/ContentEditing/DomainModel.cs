namespace Umbraco.Cms.Core.Models.ContentEditing;

/// <summary>
///     Represents a domain model for content editing operations.
/// </summary>
public class DomainModel
{
    /// <summary>
    ///     Gets or sets the domain name.
    /// </summary>
    public required string DomainName { get; set; }

    /// <summary>
    ///     Gets or sets the ISO culture code associated with the domain.
    /// </summary>
    public required string IsoCode { get; set; }
}
