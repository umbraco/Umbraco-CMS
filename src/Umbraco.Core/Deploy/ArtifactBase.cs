namespace Umbraco.Cms.Core.Deploy;

/// <summary>
/// Provides a base class for all artifacts.
/// </summary>
/// <typeparam name="TUdi">The UDI type.</typeparam>
public abstract class ArtifactBase<TUdi> : IArtifact
    where TUdi : Udi
{
    private IEnumerable<ArtifactDependency> _dependencies;
    private readonly Lazy<string> _checksum;

    /// <summary>
    /// Initializes a new instance of the <see cref="ArtifactBase{TUdi}" /> class.
    /// </summary>
    /// <param name="udi">The UDI.</param>
    /// <param name="dependencies">The dependencies.</param>
    protected ArtifactBase(TUdi udi, IEnumerable<ArtifactDependency>? dependencies = null)
    {
        Udi = udi ?? throw new ArgumentNullException(nameof(udi));
        Name = Udi.ToString();

        _dependencies = dependencies ?? Enumerable.Empty<ArtifactDependency>();
        _checksum = new Lazy<string>(GetChecksum);
    }

    /// <inheritdoc />
    Udi IArtifactSignature.Udi => Udi;

    /// <inheritdoc />
    public TUdi Udi { get; set; }

    /// <inheritdoc />
    public IEnumerable<ArtifactDependency> Dependencies
    {
        get => _dependencies;
        set => _dependencies = value.OrderBy(x => x.Udi);
    }

    /// <inheritdoc />
    public string Checksum => _checksum.Value;

    /// <inheritdoc />
    public string Name { get; set; }

    /// <inheritdoc />
    public string Alias { get; set; } = string.Empty;

    /// <summary>
    /// Gets the checksum.
    /// </summary>
    /// <returns>
    /// The checksum.
    /// </returns>
    protected abstract string GetChecksum();

    /// <summary>
    /// Prevents the <see cref="Checksum" /> property from being serialized.
    /// </summary>
    /// <returns>
    /// Returns <c>false</c> to prevent the property from being serialized.
    /// </returns>
    /// <remarks>
    /// Note that we can't use <see cref="NonSerializedAttribute" /> here as that works only on fields, not properties.  And we want to avoid using [JsonIgnore]
    /// as that would require an external dependency in Umbraco.Cms.Core.
    /// So using this method of excluding properties from serialized data, documented here: https://www.newtonsoft.com/json/help/html/ConditionalProperties.htm
    /// </remarks>
    public bool ShouldSerializeChecksum() => false;
}
