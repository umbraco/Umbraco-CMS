namespace Umbraco.Cms.Core.Routing;

/// <summary>
/// Tracks whether this server has completed the per-server initialization required to route front-end
/// content requests — i.e. the caches the content pipeline depends on (published content, URLs, domains)
/// have been seeded.
/// </summary>
/// <remarks>
/// During a background unattended upgrade the runtime level transitions to <see cref="RuntimeLevel.Run"/>
/// before <c>UmbracoApplicationStartingNotification</c> has finished seeding those caches. Serving content
/// requests in that window lets an early request observe (and cache) a negative result that then persists
/// until the process restarts. This signal lets the front-end gate content routing on "seeding complete"
/// rather than merely on <see cref="RuntimeLevel.Run"/>.
/// </remarks>
public interface IContentRoutingReadiness
{
    /// <summary>
    /// Gets a value indicating whether per-server content-routing initialization has completed.
    /// </summary>
    bool IsReady { get; }

    /// <summary>
    /// Marks per-server content-routing initialization as complete. Called once the
    /// <c>UmbracoApplicationStartingNotification</c> and its seeding handlers have finished.
    /// </summary>
    void MarkReady();
}
