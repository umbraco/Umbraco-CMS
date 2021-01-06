using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Web.Routing
{
    /// <summary>
    /// Used by <see cref="IContentFinder"/> to route inbound requests to Umbraco content
    /// </summary>
    public interface IPublishedRequestBuilder
    {
        /// <summary>
        /// Gets the cleaned up inbound Uri used for routing.
        /// </summary>
        /// <remarks>The cleaned up Uri has no virtual directory, no trailing slash, no .aspx extension, etc.</remarks>
        Uri Uri { get; }

        /// <summary>
        /// Gets the <see cref="DomainAndUri"/> assigned (if any)
        /// </summary>
        DomainAndUri Domain { get; }

        /// <summary>
        /// Gets the <see cref="CultureInfo"/> assigned (if any)
        /// </summary>
        CultureInfo Culture { get; }

        /// <summary>
        /// Gets a value indicating whether the current published content is the initial one.
        /// </summary>
        bool IsInitialPublishedContent { get; }

        /// <summary>
        /// Gets a value indicating whether the current published content has been obtained
        /// from the initial published content following internal redirections exclusively.
        /// </summary>
        /// <remarks>Used by PublishedContentRequestEngine.FindTemplate() to figure out whether to
        /// apply the internal redirect or not, when content is not the initial content.</remarks>
        bool IsInternalRedirectPublishedContent { get; }

        /// <summary>
        /// Gets the content request http response status code.
        /// </summary>
        int ResponseStatusCode { get; }

        /// <summary>
        /// Gets the current <see cref="IPublishedContent"/> assigned (if any)
        /// </summary>
        IPublishedContent PublishedContent { get; }

        /// <summary>
        /// Gets the template assigned to the request (if any)
        /// </summary>
        ITemplate Template { get; }

        /// <summary>
        /// Builds the <see cref="IPublishedRequest"/>
        /// </summary>
        IPublishedRequest Build();

        /// <summary>
        /// Sets the domain for the request
        /// </summary>
        IPublishedRequestBuilder SetDomain(DomainAndUri domain);

        /// <summary>
        /// Sets the culture for the request
        /// </summary>
        IPublishedRequestBuilder SetCulture(CultureInfo culture);

        /// <summary>
        /// Sets the found <see cref="IPublishedContent"/> for the request
        /// </summary>
        IPublishedRequestBuilder SetPublishedContent(IPublishedContent content);

        /// <summary>
        /// Sets the requested content, following an internal redirect.
        /// </summary>
        /// <param name="content">The requested content.</param>
        /// <remarks>Depending on <c>UmbracoSettings.InternalRedirectPreservesTemplate</c>, will
        /// preserve or reset the template, if any.</remarks>
        IPublishedRequestBuilder SetInternalRedirectPublishedContent(IPublishedContent content);

        /// <summary>
        /// Indicates that the current PublishedContent is the initial one.
        /// </summary>
        IPublishedRequestBuilder SetIsInitialPublishedContent(); // TODO: Required?

        /// <summary>
        /// Tries to set the template to use to display the requested content.
        /// </summary>
        /// <param name="alias">The alias of the template.</param>
        /// <returns>A value indicating whether a valid template with the specified alias was found.</returns>
        /// <remarks>
        /// <para>Successfully setting the template does refresh <c>RenderingEngine</c>.</para>
        /// <para>If setting the template fails, then the previous template (if any) remains in place.</para>
        /// </remarks>
        bool TrySetTemplate(string alias);

        /// <summary>
        /// Sets the template to use to display the requested content.
        /// </summary>
        /// <param name="template">The template.</param>
        /// <remarks>Setting the template does refresh <c>RenderingEngine</c>.</remarks>
        IPublishedRequestBuilder SetTemplate(ITemplate template);

        /// <summary>
        /// Resets the template.
        /// </summary>
        IPublishedRequestBuilder ResetTemplate();

        /// <summary>
        /// Indicates that the content request should trigger a permanent redirect (301).
        /// </summary>
        /// <param name="url">The url to redirect to.</param>
        /// <remarks>Does not actually perform a redirect, only registers that the response should
        /// redirect. Redirect will or will not take place in due time.</remarks>
        IPublishedRequestBuilder SetRedirectPermanent(string url);

        /// <summary>
        /// Indicates that the content request should trigger a redirect, with a specified status code.
        /// </summary>
        /// <param name="url">The url to redirect to.</param>
        /// <param name="status">The status code (300-308).</param>
        /// <remarks>Does not actually perform a redirect, only registers that the response should
        /// redirect. Redirect will or will not take place in due time.</remarks>
        IPublishedRequestBuilder SetRedirect(string url, int status = (int)HttpStatusCode.Redirect);

        /// <summary>
        /// Sets the http response status code, along with an optional associated description.
        /// </summary>
        /// <param name="code">The http status code.</param>
        /// <param name="description">The description.</param>
        /// <remarks>Does not actually set the http response status code and description, only registers that
        /// the response should use the specified code and description. The code and description will or will
        /// not be used, in due time.</remarks>
        IPublishedRequestBuilder SetResponseStatus(int code, string description = null);

        IPublishedRequestBuilder SetCacheabilityNoCache(bool cacheability);

        /// <summary>
        /// Sets a list of Extensions to append to the Response.Cache object.
        /// </summary>
        IPublishedRequestBuilder SetCacheExtensions(IEnumerable<string> cacheExtensions);

        /// <summary>
        /// Sets a dictionary of Headers to append to the Response object.
        /// </summary>
        IPublishedRequestBuilder SetHeaders(IReadOnlyDictionary<string, string> headers);

        /// <summary>
        /// Sets a value indicating that the requested content could not be found.
        /// </summary>
        /// <remarks>This is set in the <c>PublishedContentRequestBuilder</c> and can also be used in
        /// custom content finders or <c>Prepared</c> event handlers, where we want to allow developers
        /// to indicate a request is 404 but not to cancel it.</remarks>
        IPublishedRequestBuilder SetIs404(bool is404);

        // TODO: This seems to be the same as is404?
        //IPublishedRequestBuilder UpdateToNotFound();
    }
}
