using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Core.Models;

/// <summary>
/// Represents a domain name, optionally assigned to a content and/or language ID.
/// </summary>
/// <seealso cref="Umbraco.Cms.Core.Models.Entities.IEntity" />
/// <seealso cref="Umbraco.Cms.Core.Models.Entities.IRememberBeingDirty" />
public interface IDomain : IEntity, IRememberBeingDirty
{
    /// <summary>
    /// Gets or sets the name of the domain.
    /// </summary>
    /// <value>
    /// The name of the domain.
    /// </value>
    string DomainName { get; set; }

    /// <summary>
    /// Gets a value indicating whether this is a wildcard domain (only specifying the language of a content node).
    /// </summary>
    /// <value>
    ///   <c>true</c> if this is a wildcard domain; otherwise, <c>false</c>.
    /// </value>
    bool IsWildcard { get; }

    /// <summary>
    /// Gets or sets the language ID assigned to the domain.
    /// </summary>
    /// <value>
    /// The language ID assigned to the domain.
    /// </value>
    int? LanguageId { get; set; }

    /// <summary>
    /// Gets the language ISO code.
    /// </summary>
    /// <value>
    /// The language ISO code.
    /// </value>
    string? LanguageIsoCode { get; }

    /// <summary>
    /// Gets or sets the root content ID assigned to the domain.
    /// </summary>
    /// <value>
    /// The root content ID assigned to the domain.
    /// </value>
    int? RootContentId { get; set; }

    /// <summary>
    /// Gets or sets the sort order.
    /// </summary>
    /// <value>
    /// The sort order.
    /// </value>
    int SortOrder { get => IsWildcard ? -1 : 0; set { } } // TODO Remove default implementation in a future version
}
