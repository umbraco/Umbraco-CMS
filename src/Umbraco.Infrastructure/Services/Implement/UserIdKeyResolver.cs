using System.Collections.Concurrent;
using NPoco;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Services.Implement;

// It's okay that we never clear this, since you can never change a user's key/id
// and it'll be caught by the services if it doesn't exist.
internal sealed class UserIdKeyResolver : IUserIdKeyResolver
{
    private readonly IScopeProvider _scopeProvider;
    private readonly ConcurrentDictionary<Guid, int> _keyToId = new();
    private readonly ConcurrentDictionary<int, Guid> _idToKey = new();
    private readonly SemaphoreSlim _keytToIdLock = new(1, 1);
    private readonly SemaphoreSlim _idToKeyLock = new(1, 1);

    public UserIdKeyResolver(IScopeProvider scopeProvider) => _scopeProvider = scopeProvider;

    /// <inheritdoc/>
    public async Task<int> GetAsync(Guid key)
    {
        if (_keyToId.TryGetValue(key, out int id))
        {
            return id;
        }

        // We don't have it in the cache, so we'll need to look it up in the database
        // We'll lock, and then recheck, just to make sure the value wasn't added between the initial check and now.
        await _keytToIdLock.WaitAsync();
        try
        {
            if (_keyToId.TryGetValue(key, out int recheckedId))
            {
                // It was added while we were waiting, so we'll just return it
                return recheckedId;
            }

            // Still not here, so actually fetch it now
            using IScope scope = _scopeProvider.CreateScope(autoComplete: true);
            ISqlContext sqlContext = scope.SqlContext;

            Sql<ISqlContext> query = sqlContext.Sql()
                .Select<UserDto>(x => x.Id)
                .From<UserDto>()
                .Where<UserDto>(x => x.Key == key);

            int fetchedId = (await scope.Database.ExecuteScalarAsync<int?>(query))
                            ?? throw new InvalidOperationException("No user found with the specified key");


            _keyToId[key] = fetchedId;
            return fetchedId;
        }
        finally
        {
            _keytToIdLock.Release();
        }
    }

    /// <inheritdoc/>
    public async Task<Guid> GetAsync(int id)
    {
        if (_idToKey.TryGetValue(id, out Guid key))
        {
            return key;
        }

        await _idToKeyLock.WaitAsync();
        try
        {
            if (_idToKey.TryGetValue(id, out Guid recheckedKey))
            {
                return recheckedKey;
            }

            using IScope scope = _scopeProvider.CreateScope(autoComplete: true);
            ISqlContext sqlContext = scope.SqlContext;

            Sql<ISqlContext> query = sqlContext.Sql()
                .Select<UserDto>(x => x.Key)
                .From<UserDto>()
                .Where<UserDto>(x => x.Id == id);

            Guid fetchedKey = scope.Database.ExecuteScalar<Guid?>(query)
                              ?? throw new InvalidOperationException("No user found with the specified id");

            _idToKey[id] = fetchedKey;

            return fetchedKey;
        }
        finally
        {
            _idToKeyLock.Release();
        }
    }
}
