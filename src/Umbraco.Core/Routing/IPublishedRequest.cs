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
        /// Gets the cleaned up inbound Uri used for routing.
        /// </summary>
        /// <remarks>The cleaned up Uri has no virtual directory, no trailing slash, no .aspx extension, etc.</remarks>
        Uri Uri { get; }

        /// <summary>
        /// Gets a value indicating whether the Umbraco Backoffice should ignore a collision for this request.
        /// </summary>
        bool IgnorePublishedContentCollisions { get; }

        /// <summary>
        /// Gets a value indicating the requested content.
        /// </summary>
        IPublishedContent PublishedContent { get; }

        /// <summary>
        /// Gets the initial requested content.
        /// </summary>
        /// <remarks>The initial requested content is the content that was found by the finders,
        /// before anything such as 404, redirect... took place.</remarks>
        IPublishedContent InitialPublishedContent { get; }

        /// <summary>
        /// Gets a value indicating whether the current published content has been obtained
        /// from the initial published content following internal redirections exclusively.
        /// </summary>
        /// <remarks>Used by PublishedContentRequestEngine.FindTemplate() to figure out whether to
        /// apply the internal redirect or not, when content is not the initial content.</remarks>
        bool IsInternalRedirectPublishedContent { get; } // TODO: Not sure what thsi is yet

        /// <summary>
        /// Gets the template assigned to the request (if any)
        /// </summary>
        ITemplate Template { get; }

        /// <summary>
        /// Gets the content request's domain.
        /// </summary>
        /// <remarks>Is a DomainAndUri object ie a standard Domain plus the fully qualified uri. For example,
        /// the <c>Domain</c> may contain "example.com" whereas the <c>Uri</c> will be fully qualified eg "http://example.com/".</remarks>
        DomainAndUri Domain { get; }

        /// <summary>
        /// Gets the content request's culture.
        /// </summary>
        CultureInfo Culture { get; }

        /// <summary>
        /// Gets the url to redirect to, when the content request triggers a redirect.
        /// </summary>
        string RedirectUrl { get; }

        /// <summary>
        /// Gets the content request http response status code.
        /// </summary>
        /// <remarks>Does not actually set the http response status code, only registers that the response
        /// should use the specified code. The code will or will not be used, in due time.</remarks>
        int ResponseStatusCode { get; }

        /// <summary>
        /// Gets the content request http response status description.
        /// </summary>
        /// <remarks>Does not actually set the http response status description, only registers that the response
        /// should use the specified description. The description will or will not be used, in due time.</remarks>
        string ResponseStatusDescription { get; }

        /// <summary>
        /// Gets a list of Extensions to append to the Response.Cache object.
        /// </summary>
        IReadOnlyList<string> CacheExtensions { get; }

        /// <summary>
        /// Gets a dictionary of Headers to append to the Response object.
        /// </summary>
        IReadOnlyDictionary<string, string> Headers { get; }

        bool CacheabilityNoCache { get; }
    }
}
