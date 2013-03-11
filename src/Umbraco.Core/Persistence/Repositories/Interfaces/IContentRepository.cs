using Umbraco.Core.Models;

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
    }
}