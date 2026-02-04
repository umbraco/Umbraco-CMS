namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Represents the base model for updating a text file's content.
/// </summary>
public abstract class TextFileUpdateModel
{
    /// <summary>
    ///  The new content of the file.
    /// </summary>
    public required string Content { get; set; }
}
