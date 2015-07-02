using System.Collections.Generic;

namespace Umbraco.Core.Persistence.Repositories
{
    public interface IDeleteMediaFilesRepository
    {
        /// <summary>
        /// Called to remove all files associated with entities when an entity is permanently deleted
        /// </summary>
        /// <param name="files"></param>
        /// <returns></returns>
        bool DeleteMediaFiles(IEnumerable<string> files);
    }
}