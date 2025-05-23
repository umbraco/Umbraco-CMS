namespace Umbraco.Cms.Core.Deploy;

/// <summary>
/// Provides a base class for all artifacts.
/// </summary>
/// <typeparam name="TUdi">The UDI type.</typeparam>
public abstract class ArtifactBase<TUdi> : IArtifact
    where TUdi : Udi
{
    private IEnumerable<ArtifactDependency>? _dependencies;
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

        _dependencies = dependencies;
        _checksum = new Lazy<string>(GetChecksum);
    }

    /// <inheritdoc />
    Udi IArtifactSignature.Udi => Udi;

    /// <inheritdoc />
    public TUdi Udi { get; set; }

    /// <inheritdoc />
    public IEnumerable<ArtifactDependency> Dependencies
    {
        get => _dependencies ??= Array.Empty<ArtifactDependency>();
        set => _dependencies = value.OrderBy(x => x.Udi).ToArray();
    }

    /// <inheritdoc />
    public string Checksum => _checksum.Value;

    /// <inheritdoc />
    public string Name { get; set; }

    /// <inheritdoc />
    public string? Alias { get; set; }

    /// <summary>
    /// Gets the checksum.
    /// </summary>
    /// <returns>
    /// The checksum.
    /// </returns>
    protected abstract string GetChecksum();
}
