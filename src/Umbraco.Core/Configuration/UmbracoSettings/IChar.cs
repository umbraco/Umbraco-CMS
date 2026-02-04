namespace Umbraco.Cms.Core.Configuration.UmbracoSettings;

/// <summary>
///     Defines a character replacement mapping for URL slug generation.
/// </summary>
public interface IChar
{
    /// <summary>
    ///     Gets the character to be replaced.
    /// </summary>
    string Char { get; }

    /// <summary>
    ///     Gets the replacement string.
    /// </summary>
    string Replacement { get; }
}
