namespace Umbraco.Cms.Core.Models;

public class ScriptUpdateModel
{
    /// <summary>
    /// The new name of the file.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    ///  The new content of the file.
    /// </summary>
    public required string Content { get; set; }

    /// <summary>
    /// The path of the file to update.
    /// </summary>
    public required string ExistingPath { get; set; }
}
