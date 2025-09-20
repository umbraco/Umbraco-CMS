using Examine;
using Examine.Search;

namespace Umbraco.Cms.Api.Delivery.Services.QueryBuilders
{
    /// <summary>
    /// Used to create an <see cref="IQuery"/> instance for content items for the content delivery api.
    /// </summary>
    public interface IApiContentQueryFactory
    {
        /// <summary>
        /// Creates an <see cref="IQuery"/> for content items for the content delivery api.
        /// </summary>
        /// <param name="index">The <see cref="IIndex"/>.</param>
        /// <returns>An <see cref="IQuery"/> instance.</returns>
        IQuery CreateApiContentQuery(IIndex index);
    }
}
