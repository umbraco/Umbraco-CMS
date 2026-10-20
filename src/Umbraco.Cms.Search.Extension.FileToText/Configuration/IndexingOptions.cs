namespace Umbraco.Cms.Search.Extension.FileToText.Configuration;

/// <summary>
/// Defines options for file-to-text indexing.
/// </summary>
/// <remarks>
/// Only files referenced by the top-level properties of a content or media item are indexed. Files referenced by nested
/// properties, for example properties of blocks within a block editor, are not included.
/// </remarks>
// NOTE: include Elements handling here, if/when an Elements index ever comes around.
public sealed class IndexingOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether to include files on Content.
    /// </summary>
    public bool IncludeContent { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to include files on Media.
    /// </summary>
    public bool IncludeMedia { get; set; } = true;

    /// <summary>
    /// Gets or sets the maximum size in kb of the files to include. Larger files are skipped.
    /// </summary>
    /// <remarks>
    /// When <c>null</c>, no upper limit is applied by file-to-text indexing. The file size is then effectively limited by
    /// the maximum request length for uploads (<c>Umbraco:CMS:Runtime:MaxRequestLength</c>), if configured.
    /// </remarks>
    public long? MaxFileSize { get; set; }
}
