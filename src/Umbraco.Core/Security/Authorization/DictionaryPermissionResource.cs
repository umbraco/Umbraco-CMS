namespace Umbraco.Cms.Core.Security.Authorization;

public class DictionaryPermissionResource : IPermissionResource
{
    public DictionaryPermissionResource(IEnumerable<string> cultures) =>
        CulturesToCheck = new HashSet<string>(cultures);

    /// <summary>
    /// All the cultures need to be accessible when evaluating
    /// </summary>
    public ISet<string> CulturesToCheck { get; }
}
