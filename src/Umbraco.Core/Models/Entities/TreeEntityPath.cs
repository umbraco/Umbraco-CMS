namespace Umbraco.Cms.Core.Models.Entities;

/// <summary>
///     Represents the path of a tree entity.
/// </summary>
public class TreeEntityPath
{
    /// <summary>
    ///     Gets or sets the identifier of the entity.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    ///     Gets or sets the path of the entity.
    /// </summary>
    public string Path { get; set; } = null!;
}
