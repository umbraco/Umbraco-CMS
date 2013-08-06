using Umbraco.Core.Models;

namespace Umbraco.Core.Persistence.Repositories
{
    public interface IMediaRepository : IRepositoryVersionable<int, IMedia>
    {
        /// <summary>
        /// Empties the Recycle Bin for media Content
        /// </summary>
        /// <returns><c>True</c> if the Recycle Bin was successfully emptied and all items deleted otherwise <c>False</c></returns>
        bool EmptyRecycleBin();
    }
}