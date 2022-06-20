namespace Umbraco.Cms.Core.Models.Editors;

/// <summary>
///     Represents an uploaded file for a property.
/// </summary>
public class ContentPropertyFile
{
    /// <summary>
    ///     Gets or sets the property alias.
    /// </summary>
    public string? PropertyAlias { get; set; }

    /// <summary>
    ///     When dealing with content variants, this is the culture for the variant
    /// </summary>
    public string? Culture { get; set; }

    /// <summary>
    ///     When dealing with content variants, this is the segment for the variant
    /// </summary>
    public string? Segment { get; set; }

    /// <summary>
    ///     An array of metadata that is parsed out from the file info posted to the server which is set on the client.
    /// </summary>
    /// <remarks>
    ///     This can be used for property types like Nested Content that need to have special unique identifiers for each file
    ///     since there might be multiple files
    ///     per property.
    /// </remarks>
    public string[]? Metadata { get; set; }

    /// <summary>
    ///     Gets or sets the name of the file.
    /// </summary>
    public string? FileName { get; set; }

    /// <summary>
    ///     Gets or sets the temporary path where the file has been uploaded.
    /// </summary>
    public string TempFilePath { get; set; } = string.Empty;
}
