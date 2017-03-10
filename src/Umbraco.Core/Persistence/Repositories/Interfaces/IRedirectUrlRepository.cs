using System;
using System.Collections.Generic;
using Umbraco.Core.Models;

namespace Umbraco.Core.Persistence.Repositories
{
    /// <summary>
    /// Defines the <see cref="IRedirectUrl"/> repository.
    /// </summary>
    public interface IRedirectUrlRepository : IRepositoryQueryable<Guid, IRedirectUrl>
    {
        /// <summary>
        /// Gets a redirect url.
        /// </summary>
        /// <param name="url">The Umbraco redirect url route.</param>
        /// <param name="contentKey">The content unique key.</param>
        /// <returns></returns>
        IRedirectUrl Get(string url, Guid contentKey);

        /// <summary>
        /// Deletes a redirect url.
        /// </summary>
        /// <param name="id">The redirect url identifier.</param>
        void Delete(Guid id);

        /// <summary>
        /// Deletes all redirect urls.
        /// </summary>
        void DeleteAll();

        /// <summary>
        /// Deletes all redirect urls for a given content.
        /// </summary>
        /// <param name="contentKey">The content unique key.</param>
        void DeleteContentUrls(Guid contentKey);

        /// <summary>
        /// Gets the most recent redirect url corresponding to an Umbraco redirect url route.
        /// </summary>
        /// <param name="url">The Umbraco redirect url route.</param>
        /// <returns>The most recent redirect url corresponding to the route.</returns>
        IRedirectUrl GetMostRecentUrl(string url);

        /// <summary>
        /// Gets all redirect urls for a content item.
        /// </summary>
        /// <param name="contentKey">The content unique key.</param>
        /// <returns>All redirect urls for the content item.</returns>
        IEnumerable<IRedirectUrl> GetContentUrls(Guid contentKey);

        /// <summary>
        /// Gets all redirect urls.
        /// </summary>
        /// <param name="pageIndex">The page index.</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="total">The total count of redirect urls.</param>
        /// <returns>The redirect urls.</returns>
        IEnumerable<IRedirectUrl> GetAllUrls(long pageIndex, int pageSize, out long total);

        /// <summary>
        /// Gets all redirect urls below a given content item.
        /// </summary>
        /// <param name="rootContentId">The content unique identifier.</param>
        /// <param name="pageIndex">The page index.</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="total">The total count of redirect urls.</param>
        /// <returns>The redirect urls.</returns>
        IEnumerable<IRedirectUrl> GetAllUrls(int rootContentId, long pageIndex, int pageSize, out long total);

        /// <summary>
        /// Searches for all redirect urls that contain a given search term in their URL property.
        /// </summary>
        /// <param name="searchTerm">The term to search for.</param>
        /// <param name="pageIndex">The page index.</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="total">The total count of redirect urls.</param>
        /// <returns>The redirect urls.</returns>
        IEnumerable<IRedirectUrl> SearchUrls(string searchTerm, long pageIndex, int pageSize, out long total);
    }
}
