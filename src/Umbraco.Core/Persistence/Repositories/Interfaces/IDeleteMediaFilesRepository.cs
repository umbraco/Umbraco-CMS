using System;
using System.Collections.Generic;

namespace Umbraco.Core.Persistence.Repositories
{
    // cannot kill in v7 because it is public, kill in v8
    [Obsolete("Use MediaFileSystem.DeleteMediaFiles instead.", false)]
    public interface IDeleteMediaFilesRepository
    {
        /// <summary>
        /// Called to remove all files associated with entities when an entity is permanently deleted
        /// </summary>
        /// <param name="files"></param>
        /// <returns></returns>
        [Obsolete("Use MediaFileSystem.DeleteMediaFiles instead.", false)]
        bool DeleteMediaFiles(IEnumerable<string> files);
    }
}