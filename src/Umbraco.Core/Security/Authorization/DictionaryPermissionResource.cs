namespace Umbraco.Cms.Core.Security.Authorization;

/// <summary>
///     A resource used for the DictionaryPermissionHandler authorization handler.
/// </summary>
public class DictionaryPermissionResource : IPermissionResource
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="DictionaryPermissionResource" /> class.
    /// </summary>
    /// <param name="cultures">The cultures to check authorization for.</param>
    public DictionaryPermissionResource(IEnumerable<string> cultures) =>
        CulturesToCheck = new HashSet<string>(cultures);

    /// <summary>
    ///     Gets the cultures to check for access.
    /// </summary>
    /// <remarks>All the cultures need to be accessible when evaluating.</remarks>
    public ISet<string> CulturesToCheck { get; }
}
