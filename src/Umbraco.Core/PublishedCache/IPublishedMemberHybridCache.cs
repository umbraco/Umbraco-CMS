using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.PublishedCache;

public interface IPublishedMemberHybridCache : IPublishedMemberCache
{
    /// <summary>
    ///     Get an <see cref="IPublishedContent" /> from an <see cref="IMember" />
    /// </summary>
    /// <param name="key">The key of the member to fetch</param>
    /// <param name="preview">Will fetch draft if this is set to true</param>
    /// <returns></returns>
    Task<IPublishedMember?> GetByIdAsync(Guid key);
}
