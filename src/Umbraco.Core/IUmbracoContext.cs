using System;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Security;
using Umbraco.Web.PublishedCache;
using Umbraco.Web.Routing;

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
        /// Gets/sets the PublishedRequest object
        /// </summary>
        IPublishedRequest PublishedRequest { get; set; }

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

        IDisposable ForcedPreview(bool preview);
        void Dispose();
    }
}
