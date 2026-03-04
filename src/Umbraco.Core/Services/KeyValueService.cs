using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Scoping.EFCore;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Provides key-value storage operations for persisting simple settings.
/// </summary>
internal sealed class KeyValueService : IKeyValueService
{
    private readonly IKeyValueRepository _repository;
    private readonly IScopeProvider _scopeProvider;

    /// <summary>
    ///     Initializes a new instance of the <see cref="KeyValueService" /> class.
    /// </summary>
    /// <param name="scopeProvider">The core scope provider.</param>
    /// <param name="repository">The key-value repository.</param>
    public KeyValueService(IScopeProvider scopeProvider, IKeyValueRepository repository)
    {
        _scopeProvider = scopeProvider;
        _repository = repository;
    }

    /// <inheritdoc />
    public async Task<string?> GetValueAsync(string key)
    {
        using ICoreScope scope = _scopeProvider.CreateScope();

        IKeyValue? value = await _repository.GetAsync(key, CancellationToken.None);

        scope.Complete();
        return value?.Value;
    }

    /// <inheritdoc />
    public async Task<Attempt<IReadOnlyDictionary<string, string?>?, KeyValueOperationStatus>> FindByKeyPrefixAsync(string keyPrefix)
    {
        using ICoreScope scope = _scopeProvider.CreateScope();

        IReadOnlyDictionary<string, string?>? dict = await _repository.FindByKeyPrefixAsync(keyPrefix);

        scope.Complete();
        return Attempt.SucceedWithStatus(KeyValueOperationStatus.Success, dict);
    }

    /// <inheritdoc />
    public async Task<Attempt<KeyValueOperationStatus>> SetValueAsync(string key, string value)
    {
        using ICoreScope scope = _scopeProvider.CreateScope();
        scope.WriteLock(Constants.Locks.KeyValues);

        IKeyValue? keyValue = await _repository.GetAsync(key, CancellationToken.None);
        if (keyValue == null)
        {
            keyValue = new KeyValue { Identifier = key, Value = value, UpdateDate = DateTime.UtcNow };
        }
        else
        {
            keyValue.Value = value;
            keyValue.UpdateDate = DateTime.UtcNow;
        }

        await _repository.SaveAsync(keyValue, CancellationToken.None);

        scope.Complete();

        return Attempt.Succeed(KeyValueOperationStatus.Success);
    }

    /// <inheritdoc />
    public async Task<Attempt<KeyValueOperationStatus>> SetValueAsync(string key, string originValue, string newValue)
    {
        Attempt<bool, KeyValueOperationStatus> attempt = await TrySetValueAsync(key, originValue, newValue);
        if (!attempt.Result)
        {
            return Attempt.Fail(KeyValueOperationStatus.NoValueSet);
        }

        return Attempt.Succeed(KeyValueOperationStatus.Success);
    }

    /// <inheritdoc />
    public async Task<Attempt<bool, KeyValueOperationStatus>> TrySetValueAsync(string key, string originalValue, string newValue)
    {
        using ICoreScope scope = _scopeProvider.CreateScope();
        scope.WriteLock(Constants.Locks.KeyValues);

        IKeyValue? keyValue = await _repository.GetAsync(key, CancellationToken.None);
        if (keyValue == null || keyValue.Value != originalValue)
        {
            return Attempt.FailWithStatus(KeyValueOperationStatus.NoValueSet, false);
        }

        keyValue.Value = newValue;
        keyValue.UpdateDate = DateTime.UtcNow;
        await _repository.SaveAsync(keyValue, CancellationToken.None);

        scope.Complete();

        return Attempt.SucceedWithStatus(KeyValueOperationStatus.Success, true);
    }
}
