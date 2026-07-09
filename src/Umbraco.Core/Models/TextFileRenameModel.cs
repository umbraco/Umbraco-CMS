namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Represents the base model for renaming a text file.
/// </summary>
public abstract class TextFileRenameModel
{
    /// <summary>
    ///  The new name of the file.
    /// </summary>
    public required string Name { get; set; }
}
