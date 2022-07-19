using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.Deploy;

/// <summary>
///     Connects to an Umbraco service.
/// </summary>
public interface IServiceConnector : IDiscoverable
{
    /// <summary>
    ///     Gets an artifact.
    /// </summary>
    /// <param name="udi">The entity identifier of the artifact.</param>
    /// <returns>The corresponding artifact, or null.</returns>
    IArtifact? GetArtifact(Udi udi);

    /// <summary>
    ///     Gets an artifact.
    /// </summary>
    /// <param name="entity">The entity.</param>
    /// <returns>The corresponding artifact.</returns>
    IArtifact GetArtifact(object entity);

    /// <summary>
    ///     Initializes processing for an artifact.
    /// </summary>
    /// <param name="art">The artifact.</param>
    /// <param name="context">The deploy context.</param>
    /// <returns>The mapped artifact.</returns>
    ArtifactDeployState ProcessInit(IArtifact art, IDeployContext context);

    /// <summary>
    ///     Processes an artifact.
    /// </summary>
    /// <param name="dart">The mapped artifact.</param>
    /// <param name="context">The deploy context.</param>
    /// <param name="pass">The processing pass number.</param>
    void Process(ArtifactDeployState dart, IDeployContext context, int pass);

    /// <summary>
    ///     Explodes a range into udis.
    /// </summary>
    /// <param name="range">The range.</param>
    /// <param name="udis">The list of udis where to add the new udis.</param>
    /// <remarks>Also, it's cool to have a method named Explode. Kaboom!</remarks>
    void Explode(UdiRange range, List<Udi> udis);

    /// <summary>
    ///     Gets a named range for a specified udi and selector.
    /// </summary>
    /// <param name="udi">The udi.</param>
    /// <param name="selector">The selector.</param>
    /// <returns>The named range for the specified udi and selector.</returns>
    NamedUdiRange GetRange(Udi udi, string selector);

    /// <summary>
    ///     Gets a named range for specified entity type, identifier and selector.
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <param name="sid">The identifier.</param>
    /// <param name="selector">The selector.</param>
    /// <returns>The named range for the specified entity type, identifier and selector.</returns>
    /// <remarks>
    ///     <para>This is temporary. At least we thought it would be, in sept. 2016. What day is it now?</para>
    ///     <para>
    ///         At the moment our UI has a hard time returning proper udis, mainly because Core's tree do
    ///         not manage guids but only ints... so we have to provide a way to support it. The string id here
    ///         can be either a real string (for string udis) or an "integer as a string", using the value "-1" to
    ///         indicate the "root" i.e. an open udi.
    ///     </para>
    /// </remarks>
    NamedUdiRange GetRange(string entityType, string sid, string selector);

    /// <summary>
    ///     Compares two artifacts.
    /// </summary>
    /// <param name="art1">The first artifact.</param>
    /// <param name="art2">The second artifact.</param>
    /// <param name="differences">A collection of differences to append to, if not null.</param>
    /// <returns>A boolean value indicating whether the artifacts are identical.</returns>
    /// <remarks>ServiceConnectorBase{TArtifact} provides a very basic default implementation.</remarks>
    bool Compare(IArtifact? art1, IArtifact? art2, ICollection<Difference>? differences = null);
}
