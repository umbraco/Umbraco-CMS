namespace Umbraco.Cms.Infrastructure.Examine;

/// <summary>
///     The index types stored in the Lucene Index
/// </summary>
[Obsolete("This class will be removed in v14, please check documentation of specific search provider", true)]

public static class IndexTypes
{
    /// <summary>
    ///     The content index type
    /// </summary>
    /// <remarks>
    ///     Is lower case because the Standard Analyzer requires lower case
    /// </remarks>
    public const string Content = "content";

    /// <summary>
    ///     The media index type
    /// </summary>
    /// <remarks>
    ///     Is lower case because the Standard Analyzer requires lower case
    /// </remarks>
    public const string Media = "media";

    /// <summary>
    ///     The member index type
    /// </summary>
    /// <remarks>
    ///     Is lower case because the Standard Analyzer requires lower case
    /// </remarks>
    public const string Member = "member";
}
