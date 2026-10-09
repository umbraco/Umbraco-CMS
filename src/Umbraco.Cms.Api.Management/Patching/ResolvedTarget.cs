using System.Text.Json.Nodes;

namespace Umbraco.Cms.Api.Management.Patching;

/// <summary>
/// The result of resolving a patch path against a JSON document.
/// </summary>
public sealed class ResolvedTarget
{
    /// <summary>
    /// The parent node of the target location.
    /// </summary>
    public required JsonNode Parent { get; init; }

    /// <summary>
    /// The key identifying the target within the parent:
    /// a <see cref="string"/> for object properties, an <see cref="int"/> for array indices,
    /// or <c>null</c> for append operations.
    /// </summary>
    public required object? Key { get; init; }

    /// <summary>
    /// The current value at the target location, or null if the location doesn't exist yet (e.g., for Add).
    /// </summary>
    public JsonNode? Current { get; init; }

    /// <summary>
    /// Whether this target represents an append to the end of an array.
    /// </summary>
    public bool IsAppend { get; init; }
}
