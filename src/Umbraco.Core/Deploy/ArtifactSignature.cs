using System.Collections.Generic;
using System.Linq;

namespace Umbraco.Core.Deploy
{
    public sealed class ArtifactSignature : IArtifactSignature
    {
        public ArtifactSignature(Udi udi, string checksum, IEnumerable<ArtifactDependency> dependencies = null)
        {
            Udi = udi;
            Checksum = checksum;
            Dependencies = dependencies ?? Enumerable.Empty<ArtifactDependency>();
        }

        public Udi Udi { get; private set; }

        public string Checksum { get; private set; }

        public IEnumerable<ArtifactDependency> Dependencies { get; private set; }
    }
}