namespace Umbraco.Cms.Api.Common.DependencyInjection;

/// <summary>
///     Provides the request paths that OpenIddict should handle.
/// </summary>
/// <remarks>
///     <para>
///         Umbraco limits OpenIddict to the back-office and the well-known OpenID Connect endpoints, so
///         requests outside those paths are skipped. Implement this to add the paths of your own OpenIddict
///         endpoints, which would otherwise be skipped alongside the front-end requests.
///     </para>
///     <para>
///         Derive from <see cref="OpenIddictPathsToHandleProvider"/> to keep the Umbraco paths and add your own,
///         and register it in a composer with <c>builder.Services.AddUnique</c>.
///     </para>
/// </remarks>
public interface IOpenIddictPathsToHandleProvider
{
    /// <summary>
    ///     Gets the request paths that OpenIddict should handle.
    /// </summary>
    /// <returns>
    ///     The paths to handle. A request is handled when its path starts with any of these values, so each
    ///     value should be an absolute path beginning with a forward slash.
    /// </returns>
    IEnumerable<string> GetPathsToHandle();
}
