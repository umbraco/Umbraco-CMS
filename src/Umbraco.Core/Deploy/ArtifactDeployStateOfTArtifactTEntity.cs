namespace Umbraco.Cms.Core.Deploy;

/// <summary>
///     Represent the state of an artifact being deployed.
/// </summary>
/// <typeparam name="TArtifact">The type of the artifact.</typeparam>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
public class ArtifactDeployState<TArtifact, TEntity> : ArtifactDeployState
    where TArtifact : IArtifact
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ArtifactDeployState{TArtifact,TEntity}" /> class.
    /// </summary>
    /// <param name="art">The artifact.</param>
    /// <param name="entity">The entity.</param>
    /// <param name="connector">The service connector deploying the artifact.</param>
    /// <param name="nextPass">The next pass number.</param>
    public ArtifactDeployState(TArtifact art, TEntity? entity, IServiceConnector connector, int nextPass)
    {
        Artifact = art;
        Entity = entity;
        Connector = connector;
        NextPass = nextPass;
    }

    /// <summary>
    ///     Gets or sets the artifact.
    /// </summary>
    public new TArtifact Artifact { get; set; }

    /// <summary>
    ///     Gets or sets the entity.
    /// </summary>
    public TEntity? Entity { get; set; }

    /// <inheritdoc />
    protected sealed override IArtifact GetArtifactAsIArtifact() => Artifact;
}
