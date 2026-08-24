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
}
