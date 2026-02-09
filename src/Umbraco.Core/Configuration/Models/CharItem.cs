using Umbraco.Cms.Core.Configuration.UmbracoSettings;

namespace Umbraco.Cms.Core.Configuration.Models;

/// <summary>
///     Represents a character replacement mapping for URL slug generation.
/// </summary>
public class CharItem : IChar
{
    /// <summary>
    ///     Gets or sets the character to replace.
    /// </summary>
    public string Char { get; set; } = null!;

    /// <summary>
    ///     Gets or sets the replacement character.
    /// </summary>
    public string Replacement { get; set; } = null!;
}
