using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Persistence.Repositories;

/// <summary>
///     Represents a repository for <see cref="IKeyValue" /> entities.
/// </summary>
public interface IKeyValueRepository : IAsyncReadWriteRepository<string, IKeyValue>
{
    /// <summary>
    ///     Returns key/value pairs for all keys with the specified prefix.
    /// </summary>
    /// <param name="keyPrefix"></param>
    /// <returns></returns>
    Task<IReadOnlyDictionary<string, string?>?> FindByKeyPrefixAsync(string keyPrefix);
}
