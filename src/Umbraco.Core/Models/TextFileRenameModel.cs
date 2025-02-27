namespace Umbraco.Cms.Core.Models;

public abstract class TextFileRenameModel
{
    /// <summary>
    ///  The new name of the file.
    /// </summary>
    public required string Name { get; set; }
}
