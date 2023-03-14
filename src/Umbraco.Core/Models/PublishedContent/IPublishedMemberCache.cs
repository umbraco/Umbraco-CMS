using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.PublishedCache;

public interface IPublishedMemberCache
{
    /// <summary>
    ///     Get an <see cref="IPublishedContent" /> from an <see cref="IMember" />
    /// </summary>
    /// <param name="member"></param>
    /// <returns></returns>
    IPublishedContent? Get(IMember member);

    /// <summary>
    ///     Gets a content type identified by its unique identifier.
    /// </summary>
    /// <param name="id">The content type unique identifier.</param>
    /// <returns>The content type, or null.</returns>
    IPublishedContentType GetContentType(int id);

    /// <summary>
    ///     Gets a content type identified by its alias.
    /// </summary>
    /// <param name="alias">The content type alias.</param>
    /// <returns>The content type, or null.</returns>
    /// <remarks>The alias is case-insensitive.</remarks>
    IPublishedContentType GetContentType(string alias);
}
