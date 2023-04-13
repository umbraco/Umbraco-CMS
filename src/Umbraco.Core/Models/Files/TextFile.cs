namespace Umbraco.Cms.Core.Models;

public abstract class TextFile
{
    /// <summary>
    /// The name of the file, including the extension.
    /// </summary>
    public required string FileName;

    /// <summary>
    /// The Path to the file.
    /// </summary>
    public string? Path { get; set; }

    /// <summary>
    /// The content of the file.
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// The date the file was created.
    /// </summary>
    public DateTime CreateDate { get; set; }

    /// <summary>
    /// The date the file was last updated.
    /// </summary>
    public DateTime UpdateDate { get; set; }
}
