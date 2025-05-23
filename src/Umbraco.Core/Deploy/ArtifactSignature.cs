namespace Umbraco.Cms.Core.Deploy;

/// <inheritdoc />
public sealed class ArtifactSignature : IArtifactSignature
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ArtifactSignature" /> class.
    /// </summary>
    /// <param name="udi">The UDI.</param>
    /// <param name="checksum">The checksum.</param>
    /// <param name="dependencies">The artifact dependencies.</param>
    public ArtifactSignature(Udi udi, string checksum, IEnumerable<ArtifactDependency>? dependencies = null)
    {
        Udi = udi;
        Checksum = checksum;
        Dependencies = dependencies ?? Array.Empty<ArtifactDependency>();
    }

    /// <inheritdoc />
    public Udi Udi { get; }

    /// <inheritdoc />
    public string Checksum { get; }

    /// <inheritdoc />
    public IEnumerable<ArtifactDependency> Dependencies { get; }
}
