namespace Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;

/// <summary>
///     Represents a per-culture state row of a publishable content entity, shared by the document and element
///     culture variation tables so that repositories can build them without knowing the concrete table.
/// </summary>
internal interface ICultureVariationDto
{
    /// <summary>
    ///     Gets or sets the identifier of the content node the culture variation belongs to.
    /// </summary>
    int NodeId { get; set; }

    /// <summary>
    ///     Gets or sets the identifier of the language the culture variation is for.
    /// </summary>
    int LanguageId { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether the culture has been edited since it was last published.
    /// </summary>
    bool Edited { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether the current version has a value for the culture.
    /// </summary>
    bool Available { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether the culture is published.
    /// </summary>
    bool Published { get; set; }

    /// <summary>
    ///     Gets or sets the denormalized name of the entity in the culture.
    /// </summary>
    string? Name { get; set; }
}
