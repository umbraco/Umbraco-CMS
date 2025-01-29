using System.Runtime.CompilerServices;
using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.Deploy;

/// <summary>
/// Connects to an Umbraco service.
/// </summary>
public interface IServiceConnector : IDiscoverable
{
    /// <summary>
    /// Gets an artifact.
    /// </summary>
    /// <param name="udi">The entity identifier of the artifact.</param>
    /// <param name="contextCache">The context cache.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the corresponding artifact or <c>null</c>.
    /// </returns>
    Task<IArtifact?> GetArtifactAsync(Udi udi, IContextCache contextCache, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an artifact.
    /// </summary>
    /// <param name="entity">The entity.</param>
    /// <param name="contextCache">The context cache.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the corresponding artifact.
    /// </returns>
    Task<IArtifact> GetArtifactAsync(object entity, IContextCache contextCache, CancellationToken cancellationToken = default);

    /// <summary>
    /// Initializes processing for an artifact.
    /// </summary>
    /// <param name="artifact">The artifact.</param>
    /// <param name="context">The deploy context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the state of an artifact being deployed.
    /// </returns>
    Task<ArtifactDeployState> ProcessInitAsync(IArtifact artifact, IDeployContext context, CancellationToken cancellationToken = default);

    /// <summary>
    /// Processes an artifact.
    /// </summary>
    /// <param name="state">The state of the artifact being deployed.</param>
    /// <param name="context">The deploy context.</param>
    /// <param name="pass">The processing pass number.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// </returns>
    Task ProcessAsync(ArtifactDeployState state, IDeployContext context, int pass, CancellationToken cancellationToken = default);

    /// <summary>
    /// Expands an UDI range into UDIs.
    /// </summary>
    /// <param name="range">The UDI range.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    /// Returns an <see cref="IAsyncEnumerable{Udi}" /> which when enumerated will asynchronously expand the UDI range into UDIs.
    /// </returns>
    IAsyncEnumerable<Udi> ExpandRangeAsync(UdiRange range, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a named range for a specified UDI and selector.
    /// </summary>
    /// <param name="udi">The UDI.</param>
    /// <param name="selector">The selector.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the named range for the specified UDI and selector.
    /// </returns>
    Task<NamedUdiRange> GetRangeAsync(Udi udi, string selector, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a named range for specified entity type, identifier and selector.
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <param name="sid">The identifier.</param>
    /// <param name="selector">The selector.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the named range for the specified entity type, identifier and selector.
    /// </returns>
    /// <remarks>
    /// <para>This is temporary. At least we thought it would be, in sept. 2016. What day is it now?</para>
    /// <para>
    /// At the moment our UI has a hard time returning proper UDIs, mainly because Core's tree do
    /// not manage GUIDs but only integers... so we have to provide a way to support it. The string id here
    /// can be either a real string (for string UDIs) or an "integer as a string", using the value "-1" to
    /// indicate the "root" i.e. an open UDI.
    /// </para>
    /// </remarks>
    Task<NamedUdiRange> GetRangeAsync(string entityType, string sid, string selector, CancellationToken cancellationToken = default);

    /// <summary>
    /// Compares two artifacts.
    /// </summary>
    /// <param name="art1">The first artifact.</param>
    /// <param name="art2">The second artifact.</param>
    /// <param name="differences">A collection of differences to append to, if not <c>null</c>.</param>
    /// <returns>
    /// A boolean value indicating whether the artifacts are identical.
    /// </returns>
    bool Compare(IArtifact? art1, IArtifact? art2, ICollection<Difference>? differences = null);
}
