namespace Umbraco.Cms.Search.Core.Cache.Language;

/// <summary>
/// Defines the kinds of changes that can occur to a language.
/// </summary>
public enum LanguageChangeTypes
{
    /// <summary>
    ///     No change.
    /// </summary>
    None = 0,

    /// <summary>
    ///     A language has been deleted
    /// </summary>
    Delete = 1,
}
