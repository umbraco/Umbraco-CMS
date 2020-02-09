using System;
using System.Web;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web.PublishedCache;
using Umbraco.Web.Routing;
using Umbraco.Web.Security;

namespace Umbraco.Web
{
    public interface IUmbracoContext
    {
        /// <summary>
        /// This is used internally for performance calculations, the ObjectCreated DateTime is set as soon as this
        /// object is instantiated which in the web site is created during the BeginRequest phase.
        /// We can then determine complete rendering time from that.
        /// </summary>
        DateTime ObjectCreated { get; }

        /// <summary>
        /// This is used internally for debugging and also used to define anything required to distinguish this request from another.
        /// </summary>
        Guid UmbracoRequestId { get; }

        /// <summary>
        /// Gets the WebSecurity class
        /// </summary>
        IWebSecurity Security { get; }

        /// <summary>
        /// Gets the uri that is handled by ASP.NET after server-side rewriting took place.
        /// </summary>
        Uri OriginalRequestUrl { get; }

        /// <summary>
        /// Gets the cleaned up url that is handled by Umbraco.
        /// </summary>
        /// <remarks>That is, lowercase, no trailing slash after path, no .aspx...</remarks>
        Uri CleanedUmbracoUrl { get; }

        /// <summary>
        /// Gets the published snapshot.
        /// </summary>
        IPublishedSnapshot PublishedSnapshot { get; }

        /// <summary>
        /// Gets the published content cache.
        /// </summary>
        IPublishedContentCache Content { get; }

        /// <summary>
        /// Gets the published media cache.
        /// </summary>
        IPublishedMediaCache Media { get; }

        /// <summary>
        /// Gets the domains cache.
        /// </summary>
        IDomainCache Domains { get; }

        /// <summary>
        /// Boolean value indicating whether the current request is a front-end umbraco request
        /// </summary>
        bool IsFrontEndUmbracoRequest { get; }

        /// <summary>
        /// Gets the url provider.
        /// </summary>
        IPublishedUrlProvider UrlProvider { get; }

        /// <summary>
        /// Gets/sets the PublishedRequest object
        /// </summary>
        PublishedRequest PublishedRequest { get; set; }

        /// <summary>
        /// Exposes the HttpContext for the current request
        /// </summary>
        HttpContextBase HttpContext { get; }

        /// <summary>
        /// Gets the variation context accessor.
        /// </summary>
        IVariationContextAccessor VariationContextAccessor { get; }

        /// <summary>
        /// Gets a value indicating whether the request has debugging enabled
        /// </summary>
        /// <value><c>true</c> if this instance is debug; otherwise, <c>false</c>.</value>
        bool IsDebug { get; }

        /// <summary>
        /// Determines whether the current user is in a preview mode and browsing the site (ie. not in the admin UI)
        /// </summary>
        bool InPreviewMode { get; }

        string PreviewToken { get; }

        /// <summary>
        /// Gets the url of a content identified by its identifier.
        /// </summary>
        /// <param name="contentId">The content identifier.</param>
        /// <param name="culture"></param>
        /// <returns>The url for the content.</returns>
        string Url(int contentId, string culture = null);

        /// <summary>
        /// Gets the url of a content identified by its identifier.
        /// </summary>
        /// <param name="contentId">The content identifier.</param>
        /// <param name="culture"></param>
        /// <returns>The url for the content.</returns>
        string Url(Guid contentId, string culture = null);

        /// <summary>
        /// Gets the url of a content identified by its identifier, in a specified mode.
        /// </summary>
        /// <param name="contentId">The content identifier.</param>
        /// <param name="mode">The mode.</param>
        /// <param name="culture"></param>
        /// <returns>The url for the content.</returns>
        string Url(int contentId, UrlMode mode, string culture = null);

        /// <summary>
        /// Gets the url of a content identified by its identifier, in a specified mode.
        /// </summary>
        /// <param name="contentId">The content identifier.</param>
        /// <param name="mode">The mode.</param>
        /// <param name="culture"></param>
        /// <returns>The url for the content.</returns>
        string Url(Guid contentId, UrlMode mode, string culture = null);

        IDisposable ForcedPreview(bool preview);
        void Dispose();
    }
}
