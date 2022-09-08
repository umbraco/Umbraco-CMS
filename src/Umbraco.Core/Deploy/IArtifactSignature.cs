namespace Umbraco.Cms.Core.Deploy;

/// <summary>
///     Represents the signature of an artifact.
/// </summary>
public interface IArtifactSignature
{
    /// <summary>
    ///     Gets the entity unique identifier of this artifact.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The project identifier is independent from the state of the artifact, its data
    ///         values, dependencies, anything. It never changes and fully identifies the artifact.
    ///     </para>
    ///     <para>
    ///         What an entity uses as a unique identifier will influence what we can transfer
    ///         between environments. Eg content type "Foo" on one environment is not necessarily the
    ///         same as "Foo" on another environment, if guids are used as unique identifiers. What is
    ///         used should be documented for each entity, along with the consequences of the choice.
    ///     </para>
    /// </remarks>
    Udi Udi { get; }

    /// <summary>
    ///     Gets the checksum of this artifact.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The checksum depends on the artifact's properties, and on the identifiers of all its dependencies,
    ///         but not on their checksums. So the checksum changes when any of the artifact's properties changes,
    ///         or when the list of dependencies changes. But not if one of these dependencies change.
    ///     </para>
    ///     <para>
    ///         It is assumed that checksum collisions cannot happen ie that no two different artifact's
    ///         states will ever produce the same checksum, so that if two artifacts have the same checksum then
    ///         they are identical.
    ///     </para>
    /// </remarks>
    string Checksum { get; }

    /// <summary>
    ///     Gets the dependencies of this artifact.
    /// </summary>
    IEnumerable<ArtifactDependency> Dependencies { get; }
}
