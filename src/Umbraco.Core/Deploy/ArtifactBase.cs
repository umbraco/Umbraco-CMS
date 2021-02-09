using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Deploy
{
    /// <summary>
    /// Provides a base class to all artifacts.
    /// </summary>
    [DataContract]
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

        [DataMember]
        public TUdi Udi { get; set; }

        [IgnoreDataMember]
        public string Checksum
        {
            get { return _checksum.Value; }
        }

        [DataMember]
        public IEnumerable<ArtifactDependency> Dependencies
        {
            get { return _dependencies; }
            set { _dependencies = value.OrderBy(x => x.Udi); }
        }

        #endregion

        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Alias { get; set; }
    }
}
