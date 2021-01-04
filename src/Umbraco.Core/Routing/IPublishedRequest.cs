using System;
using System.Collections.Generic;
using System.Globalization;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Web.Routing
{
    public interface IPublishedRequest
    {
        /// <summary>
        /// Gets the UmbracoContext.
        /// </summary>
        IUmbracoContext UmbracoContext { get; } // TODO: This should be injected and removed from here

        /// <summary>
        /// Gets or sets the cleaned up Uri used for routing.
        /// </summary>
        /// <remarks>The cleaned up Uri has no virtual directory, no trailing slash, no .aspx extension, etc.</remarks>
        Uri Uri { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the Umbraco Backoffice should ignore a collision for this request.
        /// </summary>
        bool IgnorePublishedContentCollisions { get; set; }

        /// <summary>
        /// Gets or sets the requested content.
        /// </summary>
        /// <remarks>Setting the requested content clears <c>Template</c>.</remarks>
        IPublishedContent PublishedContent { get; set; }

        /// <summary>
        /// Gets the initial requested content.
        /// </summary>
        /// <remarks>The initial requested content is the content that was found by the finders,
        /// before anything such as 404, redirect... took place.</remarks>
        IPublishedContent InitialPublishedContent { get; }

        /// <summary>
        /// Gets value indicating whether the current published content is the initial one.
        /// </summary>
        bool IsInitialPublishedContent { get; }

        /// <summary>
        /// Gets or sets a value indicating whether the current published content has been obtained
        /// from the initial published content following internal redirections exclusively.
        /// </summary>
        /// <remarks>Used by PublishedContentRequestEngine.FindTemplate() to figure out whether to
        /// apply the internal redirect or not, when content is not the initial content.</remarks>
        bool IsInternalRedirectPublishedContent { get; }

        /// <summary>
        /// Gets a value indicating whether the content request has a content.
        /// </summary>
        bool HasPublishedContent { get; }

        ITemplate TemplateModel { get; set; }

        /// <summary>
        /// Gets the alias of the template to use to display the requested content.
        /// </summary>
        string TemplateAlias { get; }

        /// <summary>
        /// Gets a value indicating whether the content request has a template.
        /// </summary>
        bool HasTemplate { get; }

        void UpdateToNotFound();

        /// <summary>
        /// Gets or sets the content request's domain.
        /// </summary>
        /// <remarks>Is a DomainAndUri object ie a standard Domain plus the fully qualified uri. For example,
        /// the <c>Domain</c> may contain "example.com" whereas the <c>Uri</c> will be fully qualified eg "http://example.com/".</remarks>
        DomainAndUri Domain { get; set; }

        /// <summary>
        /// Gets a value indicating whether the content request has a domain.
        /// </summary>
        bool HasDomain { get; }

        /// <summary>
        /// Gets or sets the content request's culture.
        /// </summary>
        CultureInfo Culture { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the requested content could not be found.
        /// </summary>
        /// <remarks>This is set in the <c>PublishedContentRequestBuilder</c> and can also be used in
        /// custom content finders or <c>Prepared</c> event handlers, where we want to allow developers
        /// to indicate a request is 404 but not to cancel it.</remarks>
        bool Is404 { get; set; }

        /// <summary>
        /// Gets a value indicating whether the content request triggers a redirect (permanent or not).
        /// </summary>
        bool IsRedirect { get; }

        /// <summary>
        /// Gets or sets a value indicating whether the redirect is permanent.
        /// </summary>
        bool IsRedirectPermanent { get; }

        /// <summary>
        /// Gets or sets the url to redirect to, when the content request triggers a redirect.
        /// </summary>
        string RedirectUrl { get; }

        /// <summary>
        /// Gets or sets the content request http response status code.
        /// </summary>
        /// <remarks>Does not actually set the http response status code, only registers that the response
        /// should use the specified code. The code will or will not be used, in due time.</remarks>
        int ResponseStatusCode { get; }

        /// <summary>
        /// Gets or sets the content request http response status description.
        /// </summary>
        /// <remarks>Does not actually set the http response status description, only registers that the response
        /// should use the specified description. The description will or will not be used, in due time.</remarks>
        string ResponseStatusDescription { get; }

        /// <summary>
        /// Gets or sets a list of Extensions to append to the Response.Cache object.
        /// </summary>
        List<string> CacheExtensions { get; set; }

        /// <summary>
        /// Gets or sets a dictionary of Headers to append to the Response object.
        /// </summary>
        Dictionary<string, string> Headers { get; set; }

        bool CacheabilityNoCache { get; set; }

        /// <summary>
        /// Prepares the request.
        /// </summary>
        void Prepare();

        /// <summary>
        /// Triggers the Preparing event.
        /// </summary>
        void OnPreparing();

        /// <summary>
        /// Triggers the Prepared event.
        /// </summary>
        void OnPrepared();

        /// <summary>
        /// Sets the requested content, following an internal redirect.
        /// </summary>
        /// <param name="content">The requested content.</param>
        /// <remarks>Depending on <c>UmbracoSettings.InternalRedirectPreservesTemplate</c>, will
        /// preserve or reset the template, if any.</remarks>
        void SetInternalRedirectPublishedContent(IPublishedContent content);

        /// <summary>
        /// Indicates that the current PublishedContent is the initial one.
        /// </summary>
        void SetIsInitialPublishedContent();

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
        void SetTemplate(ITemplate template);

        /// <summary>
        /// Resets the template.
        /// </summary>
        void ResetTemplate();

        /// <summary>
        /// Indicates that the content request should trigger a redirect (302).
        /// </summary>
        /// <param name="url">The url to redirect to.</param>
        /// <remarks>Does not actually perform a redirect, only registers that the response should
        /// redirect. Redirect will or will not take place in due time.</remarks>
        void SetRedirect(string url);

        /// <summary>
        /// Indicates that the content request should trigger a permanent redirect (301).
        /// </summary>
        /// <param name="url">The url to redirect to.</param>
        /// <remarks>Does not actually perform a redirect, only registers that the response should
        /// redirect. Redirect will or will not take place in due time.</remarks>
        void SetRedirectPermanent(string url);

        /// <summary>
        /// Indicates that the content request should trigger a redirect, with a specified status code.
        /// </summary>
        /// <param name="url">The url to redirect to.</param>
        /// <param name="status">The status code (300-308).</param>
        /// <remarks>Does not actually perform a redirect, only registers that the response should
        /// redirect. Redirect will or will not take place in due time.</remarks>
        void SetRedirect(string url, int status);

        /// <summary>
        /// Sets the http response status code, along with an optional associated description.
        /// </summary>
        /// <param name="code">The http status code.</param>
        /// <param name="description">The description.</param>
        /// <remarks>Does not actually set the http response status code and description, only registers that
        /// the response should use the specified code and description. The code and description will or will
        /// not be used, in due time.</remarks>
        void SetResponseStatus(int code, string description = null);
    }
}
