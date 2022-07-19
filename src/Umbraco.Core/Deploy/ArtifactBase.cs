namespace Umbraco.Cms.Core.Deploy
{
    /// <summary>
    /// Provides a base class to all artifacts.
    /// </summary>
    public abstract class ArtifactBase<TUdi> : IArtifact
        where TUdi : Udi
    {
        protected ArtifactBase(TUdi udi, IEnumerable<ArtifactDependency>? dependencies = null)
        {
            Udi = udi ?? throw new ArgumentNullException("udi");
            Name = Udi.ToString();

            _dependencies = dependencies ?? Enumerable.Empty<ArtifactDependency>();
            _checksum = new Lazy<string>(GetChecksum);
        }

        private readonly Lazy<string> _checksum;

        private IEnumerable<ArtifactDependency> _dependencies;

        protected abstract string GetChecksum();

        Udi IArtifactSignature.Udi => Udi;

        public TUdi Udi { get; set; }

        public string Checksum => _checksum.Value;

        /// <summary>
        /// Prevents the <see cref="Checksum" /> property from being serialized.
        /// </summary>
        /// <remarks>
        /// Note that we can't use <see cref="NonSerializedAttribute"/> here as that works only on fields, not properties.  And we want to avoid using [JsonIgnore]
        /// as that would require an external dependency in Umbraco.Cms.Core.
        /// So using this method of excluding properties from serialized data, documented here: https://www.newtonsoft.com/json/help/html/ConditionalProperties.htm
        /// </remarks>
        public bool ShouldSerializeChecksum() => false;

        public IEnumerable<ArtifactDependency> Dependencies
        {
            get => _dependencies;
            set => _dependencies = value.OrderBy(x => x.Udi);
        }

        public string Name { get; set; }

        public string Alias { get; set; } = string.Empty;
    }
}
