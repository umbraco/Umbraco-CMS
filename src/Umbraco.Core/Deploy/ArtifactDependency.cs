namespace Umbraco.Cms.Core.Deploy;

/// <summary>
/// Represents an artifact dependency.
/// </summary>
public class ArtifactDependency
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ArtifactDependency" /> class.
    /// </summary>
    /// <param name="udi">The entity identifier of the artifact dependency.</param>
    /// <param name="ordering">A value indicating whether the dependency must be included when building a dependency tree and ensure the artifact gets deployed in the correct order.</param>
    /// <param name="mode">A value indicating whether the checksum must match or the artifact just needs to exist.</param>
    /// <param name="checksum">The checksum of the dependency.</param>
    public ArtifactDependency(Udi udi, bool ordering, ArtifactDependencyMode mode, string? checksum = null)
    {
        Udi = udi;
        Ordering = ordering;
        Mode = mode;
        Checksum = checksum;
    }

    /// <summary>
    /// Gets the entity identifier of the artifact dependency.
    /// </summary>
    /// <value>
    /// The entity identifier of the artifact dependency.
    /// </value>
    public Udi Udi { get; }

    /// <summary>
    /// Gets a value indicating whether the dependency is included when building a dependency tree and gets deployed in the correct order.
    /// </summary>
    /// <value>
    ///   <c>true</c> if the dependency is included when building a dependency tree and gets deployed in the correct order; otherwise, <c>false</c>.
    /// </value>
    public bool Ordering { get; }

    /// <summary>
    /// Gets the dependency mode.
    /// </summary>
    /// <value>
    /// The dependency mode.
    /// </value>
    public ArtifactDependencyMode Mode { get; }

    /// <summary>
    /// Gets or sets the checksum.
    /// </summary>
    /// <value>
    /// The checksum.
    /// </value>
    public string? Checksum { get; set; }
}
