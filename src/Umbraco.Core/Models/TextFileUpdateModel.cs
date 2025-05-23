namespace Umbraco.Cms.Core.Models;

public abstract class TextFileUpdateModel
{
    /// <summary>
    ///  The new content of the file.
    /// </summary>
    public required string Content { get; set; }
}
