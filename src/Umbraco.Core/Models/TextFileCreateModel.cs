namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Represents the base model for creating a text file.
/// </summary>
public abstract class TextFileCreateModel
{
    /// <summary>
    ///     Gets or sets the name of the file to create.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    ///     Gets or sets the parent path where the file will be created.
    /// </summary>
    /// <value>
    ///     The parent directory path, or <c>null</c> to create at the root level.
    /// </value>
    public string? ParentPath { get; set; }

    /// <summary>
    ///     Gets or sets the initial content of the file.
    /// </summary>
    /// <value>
    ///     The file content, or <c>null</c> for an empty file.
    /// </value>
    public string? Content { get; set; }
}
