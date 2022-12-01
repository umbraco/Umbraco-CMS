namespace Umbraco.Cms.Core.Deploy;

/// <summary>
/// Extension methods adding backwards-compatability between <see cref="IServiceConnector" /> and <see cref="IServiceConnector2" />.
/// </summary>
/// <remarks>
/// These extension methods will be removed in Umbraco 13.
/// </remarks>
public static class ServiceConnectorExtensions
{
    /// <summary>
    /// Gets an artifact.
    /// </summary>
    /// <param name="connector">The connector.</param>
    /// <param name="udi">The entity identifier of the artifact.</param>
    /// <param name="contextCache">The context cache.</param>
    /// <returns>
    /// The corresponding artifact, or null.
    /// </returns>
    /// <remarks>
    /// This extension method tries to make use of the <see cref="IContextCache" /> on types also implementing <see cref="IServiceConnector2" />.
    /// </remarks>
    public static IArtifact? GetArtifact(this IServiceConnector connector, Udi udi, IContextCache contextCache)
        => connector is IServiceConnector2 connector2
            ? connector2.GetArtifact(udi, contextCache)
            : connector.GetArtifact(udi);

    /// <summary>
    /// Gets an artifact.
    /// </summary>
    /// <param name="connector">The connector.</param>
    /// <param name="entity">The entity.</param>
    /// <param name="contextCache">The context cache.</param>
    /// <returns>
    /// The corresponding artifact.
    /// </returns>
    /// <remarks>
    /// This extension method tries to make use of the <see cref="IContextCache" /> on types also implementing <see cref="IServiceConnector2" />.
    /// </remarks>
    public static IArtifact GetArtifact(this IServiceConnector connector, object entity, IContextCache contextCache)
        => connector is IServiceConnector2 connector2
            ? connector2.GetArtifact(entity, contextCache)
            : connector.GetArtifact(entity);
}
