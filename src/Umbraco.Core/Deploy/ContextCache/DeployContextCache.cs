namespace Umbraco.Cms.Core.Deploy;

/// <summary>
/// A context cache that stores items on the <see cref="IDeployContext" />.
/// </summary>
/// <seealso cref="Umbraco.Cms.Core.Deploy.DictionaryCache" />
public sealed class DeployContextCache : DictionaryCache
{
    /// <summary>
    /// The prefix added to the <see cref="IDeployContext" /> item keys.
    /// </summary>
    public const string Prefix = "ContextCache.";

    /// <summary>
    /// Initializes a new instance of the <see cref="DeployContextCache" /> class.
    /// </summary>
    /// <param name="deployContext">The deploy context.</param>
    public DeployContextCache(IDeployContext deployContext)
        : base(deployContext.Items, Prefix)
    { }
}
