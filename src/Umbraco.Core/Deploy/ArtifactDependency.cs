using System.Text.Json.Serialization;

namespace Umbraco.Cms.Core.Deploy;

/// <summary>
/// Represents an artifact dependency.
/// </summary>
/// <remarks>
/// <para>
/// Dependencies have an order property which indicates whether it must be respected when ordering artifacts.
/// </para>
/// <para>
/// Dependencies have a mode which can be <see cref="ArtifactDependencyMode.Match" /> or <see cref="ArtifactDependencyMode.Exist" /> depending on whether the checksum should match.
/// </para>
/// </remarks>
public class ArtifactDependency
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ArtifactDependency" /> class.
    /// </summary>
    /// <param name="udi">The entity identifier of the artifact dependency.</param>
    /// <param name="ordering">A value indicating whether the dependency is ordering.</param>
    /// <param name="mode">The dependency mode.</param>
    /// <param name="checksum">The checksum.</param>
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
    /// Gets a value indicating whether the dependency is ordering.
    /// </summary>
    /// <value>
    ///   <c>true</c> if the dependency is ordering; otherwise, <c>false</c>.
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
    /// Gets the checksum.
    /// </summary>
    /// <value>
    /// The checksum.
    /// </value>
    public string? Checksum { get; }
}
