using System.Diagnostics.CodeAnalysis;

namespace Umbraco.Cms.Core.SchemaLockdown;

/// <summary>
/// The entity types whose schema can be governed by schema lockdown.
/// </summary>
/// <remarks>
/// The single source of truth for the governed set. Everything that enumerates or recognises a governed entity type
/// reads it from here, so bringing an entity type under lockdown is one edit rather than several that can drift apart.
/// </remarks>
public static class SchemaEntityTypes
{
    /// <summary>
    /// Every entity type schema lockdown can govern, in the exact form the decision table is keyed on.
    /// </summary>
    public static readonly IReadOnlyList<string> All =
    [
        Constants.UdiEntityType.DocumentType,
        Constants.UdiEntityType.MediaType,
        Constants.UdiEntityType.MemberType,
        Constants.UdiEntityType.DataType,
        Constants.UdiEntityType.Script,
        Constants.UdiEntityType.Stylesheet,
        Constants.UdiEntityType.DictionaryItem,
        Constants.UdiEntityType.Language,
        Constants.UdiEntityType.Webhook,
        Constants.UdiEntityType.DocumentBlueprint,
    ];

    private static readonly Dictionary<string, string> CanonicalForms =
        All.ToDictionary(entityType => entityType, StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Resolves the supplied value to the governed entity type it names, ignoring case.
    /// </summary>
    /// <param name="value">The value to resolve, typically an entry read from configuration.</param>
    /// <param name="entityType">The governed entity type, in the form <see cref="All"/> holds it.</param>
    /// <returns><c>true</c> if the value names a governed entity type; otherwise <c>false</c>.</returns>
    /// <remarks>
    /// The decision table is keyed on the exact strings in <see cref="All"/>, so a value differing only by case has
    /// to be resolved to that form before it is used as a key. Accepting such a value without resolving it would
    /// leave the entity type unlocked, because nothing would ever look the unresolved key up.
    /// </remarks>
    internal static bool TryResolve(string? value, [NotNullWhen(true)] out string? entityType)
    {
        entityType = null;
        return value is not null && CanonicalForms.TryGetValue(value, out entityType);
    }
}
