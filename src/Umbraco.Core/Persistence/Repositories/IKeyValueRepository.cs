using System.Collections.Generic;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Persistence.Repositories
{
    public interface IKeyValueRepository : IAsyncReadRepository<string, IKeyValue>, IAsyncWriteRepository<IKeyValue>
    {
        /// <summary>
        /// Returns key/value pairs for all keys with the specified prefix.
        /// </summary>
        /// <param name="keyPrefix"></param>
        /// <returns></returns>
        IReadOnlyDictionary<string, string?>? FindByKeyPrefix(string keyPrefix);
    }
}
