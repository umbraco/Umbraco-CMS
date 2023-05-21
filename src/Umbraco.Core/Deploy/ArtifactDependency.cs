namespace Umbraco.Cms.Core.Deploy;

/// <summary>
///     Represents an artifact dependency.
/// </summary>
/// <remarks>
///     <para>Dependencies have an order property which indicates whether it must be respected when ordering artifacts.</para>
///     <para>
///         Dependencies have a mode which can be <c>Match</c> or <c>Exist</c> depending on whether the checksum should
///         match.
///     </para>
/// </remarks>
public class ArtifactDependency
{
    /// <summary>
    ///     Initializes a new instance of the ArtifactDependency class with an entity identifier and a mode.
    /// </summary>
    /// <param name="udi">The entity identifier of the artifact that is a dependency.</param>
    /// <param name="ordering">A value indicating whether the dependency is ordering.</param>
    /// <param name="mode">The dependency mode.</param>
    public ArtifactDependency(Udi udi, bool ordering, ArtifactDependencyMode mode)
    {
        Udi = udi;
        Ordering = ordering;
        Mode = mode;
    }

    /// <summary>
    ///     Gets the entity id of the artifact that is a dependency.
    /// </summary>
    public Udi Udi { get; }

    /// <summary>
    ///     Gets a value indicating whether the dependency is ordering.
    /// </summary>
    public bool Ordering { get; }

    /// <summary>
    ///     Gets the dependency mode.
    /// </summary>
    public ArtifactDependencyMode Mode { get; }
}
