using System;
using System.Collections.Generic;
using Umbraco.Core.Models;

namespace Umbraco.Core.Services
{
    /// <summary>
    ///
    /// </summary>
    public interface IRedirectUrlService : IService
    {
        /// <summary>
        /// Registers a redirect url.
        /// </summary>
        /// <param name="url">The Umbraco url route.</param>
        /// <param name="contentKey">The content unique key.</param>
        /// <remarks>Is a proper Umbraco route eg /path/to/foo or 123/path/tofoo.</remarks>
        void Register(string url, Guid contentKey);

        /// <summary>
        /// Deletes all redirect urls for a given content.
        /// </summary>
        /// <param name="contentKey">The content unique key.</param>
        void DeleteContentRedirectUrls(Guid contentKey);

        /// <summary>
        /// Deletes a redirect url.
        /// </summary>
        /// <param name="redirectUrl">The redirect url to delete.</param>
        void Delete(IRedirectUrl redirectUrl);

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
        /// Gets the most recent redirect urls corresponding to an Umbraco redirect url route.
        /// </summary>
        /// <param name="url">The Umbraco redirect url route.</param>
        /// <returns>The most recent redirect urls corresponding to the route.</returns>
        IRedirectUrl GetMostRecentRedirectUrl(string url);

        /// <summary>
        /// Gets all redirect urls for a content item.
        /// </summary>
        /// <param name="contentKey">The content unique key.</param>
        /// <returns>All redirect urls for the content item.</returns>
        IEnumerable<IRedirectUrl> GetContentRedirectUrls(Guid contentKey);

        /// <summary>
        /// Gets all redirect urls.
        /// </summary>
        /// <param name="pageIndex">The page index.</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="total">The total count of redirect urls.</param>
        /// <returns>The redirect urls.</returns>
        IEnumerable<IRedirectUrl> GetAllRedirectUrls(long pageIndex, int pageSize, out long total);

        /// <summary>
        /// Gets all redirect urls below a given content item.
        /// </summary>
        /// <param name="rootContentId">The content unique identifier.</param>
        /// <param name="pageIndex">The page index.</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="total">The total count of redirect urls.</param>
        /// <returns>The redirect urls.</returns>
        IEnumerable<IRedirectUrl> GetAllRedirectUrls(int rootContentId, long pageIndex, int pageSize, out long total);

        /// <summary>
        /// Searches for all redirect urls that contain a given search term in their URL property.
        /// </summary>
        /// <param name="searchTerm">The term to search for.</param>
        /// <param name="pageIndex">The page index.</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="total">The total count of redirect urls.</param>
        /// <returns>The redirect urls.</returns>
        IEnumerable<IRedirectUrl> SearchRedirectUrls(string searchTerm, long pageIndex, int pageSize, out long total);
    }
}
