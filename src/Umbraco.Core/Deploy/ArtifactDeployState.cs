namespace Umbraco.Cms.Core.Deploy;

/// <summary>
/// Represent the state of an artifact being deployed.
/// </summary>
public abstract class ArtifactDeployState
{
    /// <summary>
    /// Gets the artifact.
    /// </summary>
    /// <value>
    /// The artifact.
    /// </value>
    public IArtifact Artifact => GetArtifactAsIArtifact();

    /// <summary>
    /// Gets or sets the service connector in charge of deploying the artifact.
    /// </summary>
    /// <value>
    /// The connector.
    /// </value>
    public IServiceConnector? Connector { get; set; }

    /// <summary>
    /// Gets or sets the next pass number.
    /// </summary>
    /// <value>
    /// The next pass.
    /// </value>
    public int NextPass { get; set; }

    /// <summary>
    /// Creates a new instance of the <see cref="ArtifactDeployState" /> class from an artifact and an entity.
    /// </summary>
    /// <typeparam name="TArtifact">The type of the artifact.</typeparam>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="art">The artifact.</param>
    /// <param name="entity">The entity.</param>
    /// <param name="connector">The service connector deploying the artifact.</param>
    /// <param name="nextPass">The next pass number.</param>
    /// <returns>
    /// A deploying artifact.
    /// </returns>
    public static ArtifactDeployState<TArtifact, TEntity> Create<TArtifact, TEntity>(TArtifact art, TEntity? entity, IServiceConnector connector, int nextPass)
        where TArtifact : IArtifact =>
        new ArtifactDeployState<TArtifact, TEntity>(art, entity, connector, nextPass);

    /// <summary>
    /// Gets the artifact as an <see cref="IArtifact" />.
    /// </summary>
    /// <returns>
    /// The artifact, as an <see cref="IArtifact" />.
    /// </returns>
    /// <remarks>
    /// This is because classes that inherit from this class cannot override the Artifact property
    /// with a property that specializes the return type, and so they need to 'new' the property.
    /// </remarks>
    protected abstract IArtifact GetArtifactAsIArtifact();
}

/// <summary>
/// Represent the state of an artifact being deployed.
/// </summary>
/// <typeparam name="TArtifact">The type of the artifact.</typeparam>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
public class ArtifactDeployState<TArtifact, TEntity> : ArtifactDeployState
    where TArtifact : IArtifact
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ArtifactDeployState{TArtifact,TEntity}" /> class.
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
    /// Gets or sets the artifact.
    /// </summary>
    /// <value>
    /// The artifact.
    /// </value>
    public new TArtifact Artifact { get; set; }

    /// <summary>
    /// Gets or sets the entity.
    /// </summary>
    /// <value>
    /// The entity.
    /// </value>
    public TEntity? Entity { get; set; }

    /// <inheritdoc />
    protected sealed override IArtifact GetArtifactAsIArtifact() => Artifact;
}
