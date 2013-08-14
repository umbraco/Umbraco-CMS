using System;
using System.Collections.Generic;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Querying;

namespace Umbraco.Core.Persistence.Repositories
{
    public interface IContentRepository : IRepositoryVersionable<int, IContent>
    {
        /// <summary>
        /// Gets a specific language version of an <see cref="IContent"/>
        /// </summary>
        /// <param name="id">Id of the <see cref="IContent"/> to retrieve version from</param>
        /// <param name="language">Culture code for the language to retrieve</param>
        /// <returns>An <see cref="IContent"/> item</returns>
        IContent GetByLanguage(int id, string language);

        /// <summary>
        /// Gets all published Content by the specified query
        /// </summary>
        /// <param name="query">Query to execute against published versions</param>
        /// <returns>An enumerable list of <see cref="IContent"/></returns>
        IEnumerable<IContent> GetByPublishedVersion(IQuery<IContent> query);
    }
}