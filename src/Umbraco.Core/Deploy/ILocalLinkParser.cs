namespace Umbraco.Cms.Core.Deploy;

/// <summary>
/// Provides methods to parse local link tags in property values.
/// </summary>
public interface ILocalLinkParser
{
    /// <summary>
    /// Parses an Umbraco property value and produces an artifact property value.
    /// </summary>
    /// <param name="value">The property value.</param>
    /// <param name="dependencies">A list of dependencies.</param>
    /// <param name="contextCache">The context cache.</param>
    /// <returns>
    /// The parsed value.
    /// </returns>
    /// <remarks>
    /// Turns {{localLink:1234}} into {{localLink:umb://{type}/{id}}} and adds the corresponding udi to the dependencies.
    /// </remarks>
    [Obsolete("Use ToArtifactAsync() instead. This method will be removed in a future version.")]
    string ToArtifact(string value, ICollection<Udi> dependencies, IContextCache contextCache);

    /// <summary>
    /// Parses an Umbraco property value and produces an artifact property value.
    /// </summary>
    /// <param name="value">The property value.</param>
    /// <param name="dependencies">A list of dependencies.</param>
    /// <param name="contextCache">The context cache.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the parsed value.
    /// </returns>
    /// <remarks>
    /// Turns {{localLink:1234}} into {{localLink:umb://{type}/{id}}} and adds the corresponding udi to the dependencies.
    /// </remarks>
    Task<string> ToArtifactAsync(string value, ICollection<Udi> dependencies, IContextCache contextCache, CancellationToken cancellationToken = default)
#pragma warning disable CS0618 // Type or member is obsolete
        => Task.FromResult(ToArtifact(value, dependencies, contextCache)); // TODO: Remove default implementation in v15
#pragma warning restore CS0618 // Type or member is obsolete

    /// <summary>
    /// Parses an artifact property value and produces an Umbraco property value.
    /// </summary>
    /// <param name="value">The artifact property value.</param>
    /// <param name="contextCache">The context cache.</param>
    /// <returns>
    /// The parsed value.
    /// </returns>
    /// <remarks>
    /// Turns {{localLink:umb://{type}/{id}}} into {{localLink:1234}}.
    /// </remarks>
    [Obsolete("Use FromArtifactAsync() instead. This method will be removed in a future version.")]
    string FromArtifact(string value, IContextCache contextCache);

    /// <summary>
    /// Parses an artifact property value and produces an Umbraco property value.
    /// </summary>
    /// <param name="value">The artifact property value.</param>
    /// <param name="contextCache">The context cache.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the parsed value.
    /// </returns>
    /// <remarks>
    /// Turns {{localLink:umb://{type}/{id}}} into {{localLink:1234}}.
    /// </remarks>
    Task<string> FromArtifactAsync(string value, IContextCache contextCache, CancellationToken cancellationToken = default)
#pragma warning disable CS0618 // Type or member is obsolete
        => Task.FromResult(FromArtifact(value, contextCache)); // TODO: Remove default implementation in v15
#pragma warning restore CS0618 // Type or member is obsolete
}
