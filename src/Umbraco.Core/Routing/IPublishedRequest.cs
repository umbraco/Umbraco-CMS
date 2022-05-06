using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.Routing;

/// <summary>
///     The result of Umbraco routing built with the <see cref="IPublishedRequestBuilder" />
/// </summary>
public interface IPublishedRequest
{
    /// <summary>
    ///     Gets the cleaned up inbound Uri used for routing.
    /// </summary>
    /// <remarks>The cleaned up Uri has no virtual directory, no trailing slash, no .aspx extension, etc.</remarks>
    Uri Uri { get; }

    /// <summary>
    ///     Gets the URI decoded absolute path of the <see cref="Uri" />
    /// </summary>
    string AbsolutePathDecoded { get; }

    /// <summary>
    ///     Gets a value indicating the requested content.
    /// </summary>
    IPublishedContent? PublishedContent { get; }

    /// <summary>
    ///     Gets a value indicating whether the current published content has been obtained
    ///     from the initial published content following internal redirections exclusively.
    /// </summary>
    /// <remarks>
    ///     Used by PublishedContentRequestEngine.FindTemplate() to figure out whether to
    ///     apply the internal redirect or not, when content is not the initial content.
    /// </remarks>
    bool IsInternalRedirect { get; }

    /// <summary>
    ///     Gets the template assigned to the request (if any)
    /// </summary>
    ITemplate? Template { get; }

    /// <summary>
    ///     Gets the content request's domain.
    /// </summary>
    /// <remarks>
    ///     Is a DomainAndUri object ie a standard Domain plus the fully qualified uri. For example,
    ///     the <c>Domain</c> may contain "example.com" whereas the <c>Uri</c> will be fully qualified eg
    ///     "http://example.com/".
    /// </remarks>
    DomainAndUri? Domain { get; }

    /// <summary>
    ///     Gets the content request's culture.
    /// </summary>
    /// <remarks>
    ///     This will get mapped to a CultureInfo eventually but CultureInfo are expensive to create so we want to leave that
    ///     up to the
    ///     localization middleware to do. See
    ///     https://github.com/dotnet/aspnetcore/blob/b795ac3546eb3e2f47a01a64feb3020794ca33bb/src/Middleware/Localization/src/RequestLocalizationMiddleware.cs#L165.
    /// </remarks>
    string? Culture { get; }

    /// <summary>
    ///     Gets the url to redirect to, when the content request triggers a redirect.
    /// </summary>
    string? RedirectUrl { get; }

    /// <summary>
    ///     Gets the content request http response status code.
    /// </summary>
    /// <remarks>
    ///     Does not actually set the http response status code, only registers that the response
    ///     should use the specified code. The code will or will not be used, in due time.
    /// </remarks>
    int? ResponseStatusCode { get; }

    /// <summary>
    ///     Gets a list of Extensions to append to the Response.Cache object.
    /// </summary>
    IReadOnlyList<string>? CacheExtensions { get; }

    /// <summary>
    ///     Gets a dictionary of Headers to append to the Response object.
    /// </summary>
    IReadOnlyDictionary<string, string>? Headers { get; }

    /// <summary>
    ///     Gets a value indicating whether the no-cache value should be added to the Cache-Control header
    /// </summary>
    bool SetNoCacheHeader { get; }

    /// <summary>
    ///     Gets a value indicating whether the Umbraco Backoffice should ignore a collision for this request.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This is an uncommon API used for edge cases with complex routing and would be used
    ///         by developers to configure the request to disable collision checks in <see cref="UrlProviderExtensions" />.
    ///     </para>
    ///     <para>
    ///         This flag is based on previous Umbraco versions but it is not clear how this flag can be set by developers
    ///         since
    ///         collission checking only occurs in the back office which is launched by
    ///         <see cref="IPublishedRouter.TryRouteRequestAsync(IPublishedRequestBuilder)" />
    ///         for which events do not execute.
    ///     </para>
    ///     <para>
    ///         More can be read about this setting here: https://github.com/umbraco/Umbraco-CMS/pull/2148,
    ///         https://issues.umbraco.org/issue/U4-10345
    ///         but it's still unclear how this was used.
    ///     </para>
    /// </remarks>
    bool IgnorePublishedContentCollisions { get; }
}
