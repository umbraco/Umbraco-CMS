using System;
using System.Collections.Generic;
using Umbraco.Core.Models;

namespace Umbraco.Core.Persistence.Repositories
{
    public interface IContentRepository : IRepositoryQueryable<int, IContent>
    {
        /// <summary>
        /// Gets a list of all versions for an <see cref="IContent"/>.
        /// </summary>
        /// <param name="id">Id of the <see cref="IContent"/> to retrieve versions from</param>
        /// <returns>An enumerable list of the same <see cref="IContent"/> object with different versions</returns>
        IEnumerable<IContent> GetAllVersions(int id);

        /// <summary>
        /// Gets a specific version of an <see cref="IContent"/>.
        /// </summary>
        /// <param name="id">Id of the <see cref="IContent"/> to retrieve version from</param>
        /// <param name="versionId">Id of the version to retrieve</param>
        /// <returns>An <see cref="IContent"/> item</returns>
        IContent GetByVersion(int id, Guid versionId);

        /// <summary>
        /// Deletes a specific version from an <see cref="IContent"/> object.
        /// </summary>
        /// <param name="id">Id of the <see cref="IContent"/> object to delete a version from</param>
        /// <param name="versionId">Id of the version to delete</param>
        void Delete(int id, Guid versionId);

        /// <summary>
        /// Deletes versions from an <see cref="IContent"/> object prior to a specific date.
        /// </summary>
        /// <param name="id">Id of the <see cref="IContent"/> object to delete versions from</param>
        /// <param name="versionDate">Latest version date</param>
        void Delete(int id, DateTime versionDate);
    }
}