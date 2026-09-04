using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Security.Authorization;

/// <summary>
///     A resource used for the DocumentBlueprintPermissionHandler authorization handler.
/// </summary>
/// <remarks>
///     Document blueprints have no recycle bin, so unlike the content and media resources this one has no
///     recycle bin to check.
/// </remarks>
public class DocumentBlueprintPermissionResource : IPermissionResource
{
    private DocumentBlueprintPermissionResource(IEnumerable<Guid> documentBlueprintKeys, bool checkRoot)
    {
        DocumentBlueprintKeys = documentBlueprintKeys;
        CheckRoot = checkRoot;
    }

    /// <summary>
    ///     Gets the keys to authorize, each identifying either a document blueprint or one of the containers
    ///     holding them.
    /// </summary>
    public IEnumerable<Guid> DocumentBlueprintKeys { get; }

    /// <summary>
    ///     Gets a value indicating whether to check the root.
    /// </summary>
    public bool CheckRoot { get; }

    /// <summary>
    ///     Creates a <see cref="DocumentBlueprintPermissionResource" /> with the specified key.
    /// </summary>
    /// <param name="documentBlueprintKey">The key of the document blueprint or null if root.</param>
    /// <returns>An instance of <see cref="DocumentBlueprintPermissionResource" />.</returns>
    public static DocumentBlueprintPermissionResource WithKeys(Guid? documentBlueprintKey) =>
        documentBlueprintKey is null
            ? Root()
            : WithKeys(documentBlueprintKey.Value.Yield());

    /// <summary>
    ///     Creates a <see cref="DocumentBlueprintPermissionResource" /> with the specified key.
    /// </summary>
    /// <param name="documentBlueprintKey">The key of the document blueprint.</param>
    /// <returns>An instance of <see cref="DocumentBlueprintPermissionResource" />.</returns>
    public static DocumentBlueprintPermissionResource WithKeys(Guid documentBlueprintKey) =>
        WithKeys(documentBlueprintKey.Yield());

    /// <summary>
    ///     Creates a <see cref="DocumentBlueprintPermissionResource" /> with the specified keys.
    /// </summary>
    /// <param name="documentBlueprintKeys">The keys of the document blueprints.</param>
    /// <returns>An instance of <see cref="DocumentBlueprintPermissionResource" />.</returns>
    public static DocumentBlueprintPermissionResource WithKeys(IEnumerable<Guid> documentBlueprintKeys) =>
        new(documentBlueprintKeys, false);

    /// <summary>
    ///     Creates a <see cref="DocumentBlueprintPermissionResource" /> with the specified keys, where a null
    ///     entry stands for the root.
    /// </summary>
    /// <param name="documentBlueprintKeys">The keys to authorize, or null for the root.</param>
    /// <returns>An instance of <see cref="DocumentBlueprintPermissionResource" />.</returns>
    /// <remarks>
    ///     Used where an operation has more than one end to authorize, such as a move, which has to check both
    ///     the blueprint and the container it is moving into or a user confined to a subtree could move a
    ///     blueprint out of it.
    /// </remarks>
    public static DocumentBlueprintPermissionResource WithKeys(IEnumerable<Guid?> documentBlueprintKeys)
    {
        IEnumerable<Guid?> keyList = documentBlueprintKeys.ToList();
        var hasRoot = keyList.Any(x => x is null);
        IEnumerable<Guid> keys = keyList.Where(x => x.HasValue).Select(x => x!.Value);
        return new DocumentBlueprintPermissionResource(keys, hasRoot);
    }

    /// <summary>
    ///     Creates a <see cref="DocumentBlueprintPermissionResource" /> with the root.
    /// </summary>
    /// <returns>An instance of <see cref="DocumentBlueprintPermissionResource" />.</returns>
    public static DocumentBlueprintPermissionResource Root() =>
        new(Enumerable.Empty<Guid>(), true);
}
