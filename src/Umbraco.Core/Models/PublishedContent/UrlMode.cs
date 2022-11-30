namespace Umbraco.Cms.Core.Models.PublishedContent;

/// <summary>
///     Specifies the type of URLs that the URL provider should produce, Auto is the default.
/// </summary>
public enum UrlMode
{
    /// <summary>
    ///     Indicates that the URL provider should do what it has been configured to do.
    /// </summary>
    Default = 0,

    /// <summary>
    ///     Indicates that the URL provider should produce relative URLs exclusively.
    /// </summary>
    Relative,

    /// <summary>
    ///     Indicates that the URL provider should produce absolute URLs exclusively.
    /// </summary>
    Absolute,

    /// <summary>
    ///     Indicates that the URL provider should determine automatically whether to return relative or absolute URLs.
    /// </summary>
    Auto,
}
