namespace Umbraco.Cms.Core.Models.TemporaryFile;

/// <summary>
///     Represents the base model for a temporary file.
/// </summary>
public abstract class TemporaryFileModelBase
{
    /// <summary>
    ///     Gets or sets the name of the file.
    /// </summary>
    public required string FileName { get; set; }

    /// <summary>
    ///     Gets or sets the unique key identifying the temporary file.
    /// </summary>
    public Guid Key { get; set; }

    /// <summary>
    ///     Gets or sets the function to open a read stream for the file content.
    /// </summary>
    public Func<Stream> OpenReadStream { get; set; } = () => Stream.Null;
}
