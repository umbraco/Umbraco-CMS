namespace Umbraco.Cms.Api.Common.Configuration;

/// <summary>
///     Options for request paths that OpenIddict should handle in addition to the Umbraco ones.
/// </summary>
/// <remarks>
///     Umbraco limits OpenIddict to the back-office and the well-known OpenID Connect endpoints, and skips every
///     other request. Add the paths of your own OpenIddict endpoints here, for example with
///     <c>builder.Services.Configure&lt;OpenIddictAdditionalPathsOptions&gt;(o =&gt; o.PathsToHandle.Add("/my-own-oidc/"))</c>.
///     The Umbraco paths are always handled and cannot be removed through these options.
/// </remarks>
public class OpenIddictAdditionalPathsOptions
{
    /// <summary>
    ///     Gets the additional request paths that OpenIddict should handle.
    /// </summary>
    /// <remarks>
    ///     A request is handled when its path starts with one of these values, using a case-sensitive ordinal
    ///     comparison. Use absolute paths that start and end with a forward slash, such as <c>/my-own-oidc/</c>, so a
    ///     value does not also match a longer segment like <c>/my-own-oidc-other</c>. Empty values are ignored. A value
    ///     of <c>/</c> matches every request, which lets OpenIddict process front-end requests as well.
    /// </remarks>
    public IList<string> PathsToHandle { get; } = new List<string>();
}
