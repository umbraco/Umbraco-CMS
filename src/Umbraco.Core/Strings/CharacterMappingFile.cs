namespace Umbraco.Cms.Core.Strings;

/// <summary>
/// Represents a character mapping JSON file.
/// </summary>
internal sealed class CharacterMappingFile
{
    /// <summary>
    /// Name of the mapping set.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Optional description.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Priority for override ordering. Higher values override lower.
    /// </summary>
    public int Priority { get; init; }

    /// <summary>
    /// Character to string mappings.
    /// </summary>
    public required Dictionary<string, string> Mappings { get; init; }
}
