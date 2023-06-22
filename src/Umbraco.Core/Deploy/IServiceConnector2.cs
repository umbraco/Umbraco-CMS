namespace Umbraco.Cms.Core.Deploy;

/// <inheritdoc />
/// <remarks>
/// This interface will be merged back into <see cref="IServiceConnector" /> and removed in Umbraco 13.
/// </remarks>
public interface IServiceConnector2 : IServiceConnector
{
    /// <inheritdoc />
    [Obsolete($"Use the overload accepting {nameof(IContextCache)} instead. This overload will be removed in Umbraco 13.")]
    IArtifact? IServiceConnector.GetArtifact(Udi udi)
        => GetArtifact(udi, PassThroughCache.Instance);

    /// <summary>
    /// Gets an artifact.
    /// </summary>
    /// <param name="udi">The entity identifier of the artifact.</param>
    /// <param name="contextCache">The context cache.</param>
    /// <returns>
    /// The corresponding artifact, or null.
    /// </returns>
    IArtifact? GetArtifact(Udi udi, IContextCache contextCache);

    /// <inheritdoc />
    [Obsolete($"Use the overload accepting {nameof(IContextCache)} instead. This overload will be removed in Umbraco 13.")]
    IArtifact IServiceConnector.GetArtifact(object entity)
       => GetArtifact(entity, PassThroughCache.Instance);

    /// <summary>
    /// Gets an artifact.
    /// </summary>
    /// <param name="entity">The entity.</param>
    /// <param name="contextCache">The context cache.</param>
    /// <returns>
    /// The corresponding artifact.
    /// </returns>
    IArtifact GetArtifact(object entity, IContextCache contextCache);
}
