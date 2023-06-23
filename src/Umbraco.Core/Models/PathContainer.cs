namespace Umbraco.Cms.Core.Models;

/// <summary>
/// A container using a path as its identity.
/// </summary>
public class PathContainer
{
    /// <summary>
    /// The name of the container
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// The path to the parent of the container
    /// </summary>
    public string? ParentPath { get; set; }

    /// <summary>
    /// The path of the container.
    /// </summary>
    public string Path =>
        string.IsNullOrEmpty(ParentPath)
            ? Name
            : System.IO.Path.Combine(ParentPath, Name);
}
