using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Umbraco.Core.Deploy
{
    /// <summary>
    /// Provides a base class to all artifacts.
    /// </summary>
    public abstract class ArtifactBase<TUdi> : IArtifact
        where TUdi : Udi
    {
        protected ArtifactBase(TUdi udi, IEnumerable<ArtifactDependency> dependencies = null)
        {
            if (udi == null)
                throw new ArgumentNullException("udi");
            Udi = udi;
            Name = Udi.ToString();

            Dependencies = dependencies ?? Enumerable.Empty<ArtifactDependency>();
            _checksum = new Lazy<string>(GetChecksum);
        }

        private readonly Lazy<string> _checksum;
        private IEnumerable<ArtifactDependency> _dependencies;

        protected abstract string GetChecksum();

        #region Abstract implementation of IArtifactSignature

        Udi IArtifactSignature.Udi
        {
            get { return Udi; }
        }

        public TUdi Udi { get; set; }

        [JsonIgnore]
        public string Checksum
        {
            get { return _checksum.Value; }
        }

        public IEnumerable<ArtifactDependency> Dependencies
        {
            get { return _dependencies; }
            set { _dependencies = value.OrderBy(x => x.Udi); }
        }

        #endregion

        public string Name { get; set; }
        public string Alias { get; set; }
    }
}