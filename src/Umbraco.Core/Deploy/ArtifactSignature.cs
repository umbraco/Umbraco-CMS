namespace Umbraco.Cms.Core.Deploy;

public sealed class ArtifactSignature : IArtifactSignature
{
    public ArtifactSignature(Udi udi, string checksum, IEnumerable<ArtifactDependency>? dependencies = null)
    {
        Udi = udi;
        Checksum = checksum;
        Dependencies = dependencies ?? Enumerable.Empty<ArtifactDependency>();
    }

    public Udi Udi { get; }

    public string Checksum { get; }

    public IEnumerable<ArtifactDependency> Dependencies { get; }
}
