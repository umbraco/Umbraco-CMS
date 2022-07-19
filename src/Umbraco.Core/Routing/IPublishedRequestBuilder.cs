using System.Globalization;
using System.Net;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.Routing;

/// <summary>
///     Used by <see cref="IContentFinder" /> to route inbound requests to Umbraco content
/// </summary>
public interface IPublishedRequestBuilder
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
    ///     Gets the <see cref="DomainAndUri" /> assigned (if any)
    /// </summary>
    DomainAndUri? Domain { get; }

    /// <summary>
    ///     Gets the <see cref="CultureInfo" /> assigned (if any)
    /// </summary>
    string? Culture { get; }

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
    ///     Gets the content request http response status code.
    /// </summary>
    int? ResponseStatusCode { get; }

    /// <summary>
    ///     Gets the current <see cref="IPublishedContent" /> assigned (if any)
    /// </summary>
    IPublishedContent? PublishedContent { get; }

    /// <summary>
    ///     Gets the template assigned to the request (if any)
    /// </summary>
    ITemplate? Template { get; }

    /// <summary>
    ///     Builds the <see cref="IPublishedRequest" />
    /// </summary>
    IPublishedRequest Build();

    /// <summary>
    ///     Sets the domain for the request which also sets the culture
    /// </summary>
    IPublishedRequestBuilder SetDomain(DomainAndUri domain);

    /// <summary>
    ///     Sets the culture for the request
    /// </summary>
    IPublishedRequestBuilder SetCulture(string? culture);

    /// <summary>
    ///     Sets the found <see cref="IPublishedContent" /> for the request
    /// </summary>
    /// <remarks>Setting the content clears the template and redirect</remarks>
    IPublishedRequestBuilder SetPublishedContent(IPublishedContent? content);

    /// <summary>
    ///     Sets the requested content, following an internal redirect.
    /// </summary>
    /// <param name="content">The requested content.</param>
    /// <remarks>Since this sets the content, it will clear the template</remarks>
    IPublishedRequestBuilder SetInternalRedirect(IPublishedContent content);

    /// <summary>
    ///     Tries to set the template to use to display the requested content.
    /// </summary>
    /// <param name="alias">The alias of the template.</param>
    /// <returns>A value indicating whether a valid template with the specified alias was found.</returns>
    /// <remarks>
    ///     <para>Successfully setting the template does refresh <c>RenderingEngine</c>.</para>
    ///     <para>If setting the template fails, then the previous template (if any) remains in place.</para>
    /// </remarks>
    bool TrySetTemplate(string alias);

    /// <summary>
    ///     Sets the template to use to display the requested content.
    /// </summary>
    /// <param name="template">The template.</param>
    /// <remarks>Setting the template does refresh <c>RenderingEngine</c>.</remarks>
    IPublishedRequestBuilder SetTemplate(ITemplate? template);

    /// <summary>
    ///     Indicates that the content request should trigger a permanent redirect (301).
    /// </summary>
    /// <param name="url">The url to redirect to.</param>
    /// <remarks>
    ///     Does not actually perform a redirect, only registers that the response should
    ///     redirect. Redirect will or will not take place in due time.
    /// </remarks>
    IPublishedRequestBuilder SetRedirectPermanent(string url);

    /// <summary>
    ///     Indicates that the content request should trigger a redirect, with a specified status code.
    /// </summary>
    /// <param name="url">The url to redirect to.</param>
    /// <param name="status">The status code (300-308).</param>
    /// <remarks>
    ///     Does not actually perform a redirect, only registers that the response should
    ///     redirect. Redirect will or will not take place in due time.
    /// </remarks>
    IPublishedRequestBuilder SetRedirect(string url, int status = (int)HttpStatusCode.Redirect);

    /// <summary>
    ///     Sets the http response status code, along with an optional associated description.
    /// </summary>
    /// <param name="code">The http status code.</param>
    /// <remarks>
    ///     Does not actually set the http response status code and description, only registers that
    ///     the response should use the specified code and description. The code and description will or will
    ///     not be used, in due time.
    /// </remarks>
    IPublishedRequestBuilder SetResponseStatus(int code);

    /// <summary>
    ///     Sets the no-cache value to the Cache-Control header
    /// </summary>
    /// <param name="setHeader">True to set the header, false to not set it</param>
    IPublishedRequestBuilder SetNoCacheHeader(bool setHeader);

    /// <summary>
    ///     Sets a list of Extensions to append to the Response.Cache object.
    /// </summary>
    IPublishedRequestBuilder SetCacheExtensions(IEnumerable<string> cacheExtensions);

    /// <summary>
    ///     Sets a dictionary of Headers to append to the Response object.
    /// </summary>
    IPublishedRequestBuilder SetHeaders(IReadOnlyDictionary<string, string> headers);

    /// <summary>
    ///     Can be called to configure the <see cref="IPublishedRequest" /> result to ignore URL collisions
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
    void IgnorePublishedContentCollisions();
}
