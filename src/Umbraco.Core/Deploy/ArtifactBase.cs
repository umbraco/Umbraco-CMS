using System;
using System.Collections.Generic;
using System.Linq;

namespace Umbraco.Cms.Core.Deploy
{
    /// <summary>
    /// Provides a base class to all artifacts.
    /// </summary>
    public abstract class ArtifactBase<TUdi> : IArtifact
        where TUdi : Udi
    {
        protected ArtifactBase(TUdi udi, IEnumerable<ArtifactDependency> dependencies = null)
        {
            Udi = udi ?? throw new ArgumentNullException("udi");
            Name = Udi.ToString();

            Dependencies = dependencies ?? Enumerable.Empty<ArtifactDependency>();
            _checksum = new Lazy<string>(GetChecksum);
        }

        private readonly Lazy<string> _checksum;

        private IEnumerable<ArtifactDependency> _dependencies;

        protected abstract string GetChecksum();

        #region Abstract implementation of IArtifactSignature

        Udi IArtifactSignature.Udi => Udi;

        public TUdi Udi { get; set; }

        public string Checksum => _checksum.Value;

        public bool ShouldSerializeChecksum() => false;

        public IEnumerable<ArtifactDependency> Dependencies
        {
            get => _dependencies;
            set => _dependencies = value.OrderBy(x => x.Udi);
        }

        #endregion

        public string Name { get; set; }

        public string Alias { get; set; }
    }
}
