using System.Collections.Concurrent;
using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Services.Implement;

/// <summary>
/// Implements <see cref="IUserIdKeyResolver"/> for resolving user keys to user IDs and vice versa without retrieving full user details.
/// </summary>
/// <remarks>
/// It's okay that we never clear this, since you can never change a user's key/id
/// and it'll be caught by the services if it doesn't exist.
/// </remarks>
internal sealed class UserIdKeyResolver : IUserIdKeyResolver
{
    private readonly IScopeProvider _scopeProvider;
    private readonly ConcurrentDictionary<Guid, int> _keyToId = new();
    private readonly ConcurrentDictionary<int, Guid> _idToKey = new();
    private readonly SemaphoreSlim _keytToIdLock = new(1, 1);
    private readonly SemaphoreSlim _idToKeyLock = new(1, 1);

    /// <summary>
    /// Initializes a new instance of the <see cref="UserIdKeyResolver"/> class.
    /// </summary>
    public UserIdKeyResolver(IScopeProvider scopeProvider) => _scopeProvider = scopeProvider;

    /// <inheritdoc/>
    public async Task<int> GetAsync(Guid key)
        => await TryGetAsync(key) is { Success: true } attempt ? attempt.Result : throw new InvalidOperationException("No user found with the specified key");

    /// <inheritdoc/>
    public async Task<Attempt<int>> TryGetAsync(Guid key)
    {
        // The super-user Id and key is known, so we don't need a look-up here.
        if (key == Constants.Security.SuperUserKey)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            return Attempt.Succeed(Constants.Security.SuperUserId);
#pragma warning restore CS0618 // Type or member is obsolete
        }

        if (_keyToId.TryGetValue(key, out int id))
        {
            return Attempt.Succeed(id);
        }

        // We don't have it in the cache, so we'll need to look it up in the database
        // We'll lock, and then recheck, just to make sure the value wasn't added between the initial check and now.
        await _keytToIdLock.WaitAsync();
        try
        {
            if (_keyToId.TryGetValue(key, out int recheckedId))
            {
                // It was added while we were waiting, so we'll just return it
                return Attempt.Succeed(recheckedId);
            }

            // Still not here, so actually fetch it now
            using IScope scope = _scopeProvider.CreateScope(autoComplete: true);
            ISqlContext sqlContext = scope.SqlContext;

            Sql<ISqlContext> query = sqlContext.Sql()
                .Select<UserDto>(x => x.Id)
                .From<UserDto>()
                .Where<UserDto>(x => x.Key == key);

            int? fetchedId = await scope.Database.ExecuteScalarAsync<int?>(query);
            if (fetchedId is null)
            {
                return Attempt.Fail<int>();
            }

            _keyToId[key] = fetchedId.Value;
            return Attempt.Succeed(fetchedId.Value);
        }
        finally
        {
            _keytToIdLock.Release();
        }
    }

    /// <inheritdoc/>
    public async Task<Guid> GetAsync(int id)
        => await TryGetAsync(id) is { Success: true } attempt ? attempt.Result : throw new InvalidOperationException("No user found with the specified id");

    /// <inheritdoc/>
    public async Task<Attempt<Guid>> TryGetAsync(int id)
    {
        // The super-user Id and key is known, so we don't need a look-up here.
#pragma warning disable CS0618 // Type or member is obsolete
        if (id is Constants.Security.SuperUserId)
#pragma warning restore CS0618 // Type or member is obsolete
        {
            return Attempt.Succeed(Constants.Security.SuperUserKey);
        }

        if (_idToKey.TryGetValue(id, out Guid key))
        {
            return Attempt.Succeed(key);
        }

        await _idToKeyLock.WaitAsync();
        try
        {
            if (_idToKey.TryGetValue(id, out Guid recheckedKey))
            {
                return Attempt.Succeed(recheckedKey);
            }

            using IScope scope = _scopeProvider.CreateScope(autoComplete: true);
            ISqlContext sqlContext = scope.SqlContext;

            Sql<ISqlContext> query = sqlContext.Sql()
                .Select<UserDto>(x => x.Key)
                .From<UserDto>()
                .Where<UserDto>(x => x.Id == id);

            Guid? fetchedKey = scope.Database.FirstOrDefault<Guid?>(query);
            if (fetchedKey is null)
            {
                return Attempt<Guid>.Fail();
            }

            _idToKey[id] = fetchedKey.Value;

            return Attempt.Succeed(fetchedKey.Value);
        }
        finally
        {
            _idToKeyLock.Release();
        }
    }
}
