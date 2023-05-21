using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;

namespace Umbraco.Cms.Core.Web;

public interface IUmbracoContext : IDisposable
{
    /// <summary>
    ///     Gets the DateTime this instance was created.
    /// </summary>
    /// <remarks>
    ///     Used internally for performance calculations, the ObjectCreated DateTime is set as soon as this
    ///     object is instantiated which in the web site is created during the BeginRequest phase.
    ///     We can then determine complete rendering time from that.
    /// </remarks>
    DateTime ObjectCreated { get; }

    /// <summary>
    ///     Gets the uri that is handled by ASP.NET after server-side rewriting took place.
    /// </summary>
    Uri OriginalRequestUrl { get; }

    /// <summary>
    ///     Gets the cleaned up url that is handled by Umbraco.
    /// </summary>
    /// <remarks>That is, lowercase, no trailing slash after path, no .aspx...</remarks>
    Uri CleanedUmbracoUrl { get; }

    /// <summary>
    ///     Gets the published snapshot.
    /// </summary>
    IPublishedSnapshot PublishedSnapshot { get; }

    /// <summary>
    ///     Gets the published content cache.
    /// </summary>
    IPublishedContentCache? Content { get; }

    /// <summary>
    ///     Gets the published media cache.
    /// </summary>
    IPublishedMediaCache? Media { get; }

    /// <summary>
    ///     Gets the domains cache.
    /// </summary>
    IDomainCache? Domains { get; }

    /// <summary>
    ///     Gets or sets the PublishedRequest object
    /// </summary>
    //// TODO: Can we refactor this so it's not a settable thing required for routing?
    //// The only nicer way would be to have a RouteRequest method directly on IUmbracoContext but that means adding another dep to the ctx/factory.
    IPublishedRequest? PublishedRequest { get; set; }

    /// <summary>
    ///     Gets a value indicating whether the request has debugging enabled
    /// </summary>
    /// <value><c>true</c> if this instance is debug; otherwise, <c>false</c>.</value>
    bool IsDebug { get; }

    /// <summary>
    ///     Gets a value indicating whether the current user is in a preview mode and browsing the site (ie. not in the admin UI)
    /// </summary>
    bool InPreviewMode { get; }

    /// <summary>
    ///     Forces the context into preview
    /// </summary>
    /// <returns>A <see cref="IDisposable" /> instance to be disposed to exit the preview context</returns>
    IDisposable ForcedPreview(bool preview);
}
