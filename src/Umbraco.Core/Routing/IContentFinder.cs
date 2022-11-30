namespace Umbraco.Cms.Core.Routing;

/// <summary>
///     Provides a method to try to find and assign an Umbraco document to a <c>PublishedRequest</c>.
/// </summary>
public interface IContentFinder
{
    /// <summary>
    ///     Tries to find and assign an Umbraco document to a <c>PublishedRequest</c>.
    /// </summary>
    /// <param name="request">The <c>PublishedRequest</c>.</param>
    /// <returns>A value indicating whether an Umbraco document was found and assigned.</returns>
    /// <remarks>
    ///     Optionally, can also assign the template or anything else on the document request, although that is not
    ///     required.
    /// </remarks>
    Task<bool> TryFindContent(IPublishedRequestBuilder request);
}
