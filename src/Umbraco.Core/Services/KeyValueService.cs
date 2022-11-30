using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;

namespace Umbraco.Cms.Core.Services;

internal class KeyValueService : IKeyValueService
{
    private readonly IKeyValueRepository _repository;
    private readonly ICoreScopeProvider _scopeProvider;

    public KeyValueService(ICoreScopeProvider scopeProvider, IKeyValueRepository repository)
    {
        _scopeProvider = scopeProvider;
        _repository = repository;
    }

    /// <inheritdoc />
    public string? GetValue(string key)
    {
        using (ICoreScope scope = _scopeProvider.CreateCoreScope(autoComplete: true))
        {
            return _repository.Get(key)?.Value;
        }
    }

    /// <inheritdoc />
    public IReadOnlyDictionary<string, string?>? FindByKeyPrefix(string keyPrefix)
    {
        using (ICoreScope scope = _scopeProvider.CreateCoreScope(autoComplete: true))
        {
            return _repository.FindByKeyPrefix(keyPrefix);
        }
    }

    /// <inheritdoc />
    public void SetValue(string key, string value)
    {
        using (ICoreScope scope = _scopeProvider.CreateCoreScope())
        {
            scope.WriteLock(Constants.Locks.KeyValues);

            IKeyValue? keyValue = _repository.Get(key);
            if (keyValue == null)
            {
                keyValue = new KeyValue { Identifier = key, Value = value, UpdateDate = DateTime.Now };
            }
            else
            {
                keyValue.Value = value;
                keyValue.UpdateDate = DateTime.Now;
            }

            _repository.Save(keyValue);

            scope.Complete();
        }
    }

    /// <inheritdoc />
    public void SetValue(string key, string originValue, string newValue)
    {
        if (!TrySetValue(key, originValue, newValue))
        {
            throw new InvalidOperationException("Could not set the value.");
        }
    }

    /// <inheritdoc />
    public bool TrySetValue(string key, string originalValue, string newValue)
    {
        using (ICoreScope scope = _scopeProvider.CreateCoreScope())
        {
            scope.WriteLock(Constants.Locks.KeyValues);

            IKeyValue? keyValue = _repository.Get(key);
            if (keyValue == null || keyValue.Value != originalValue)
            {
                return false;
            }

            keyValue.Value = newValue;
            keyValue.UpdateDate = DateTime.Now;
            _repository.Save(keyValue);

            scope.Complete();
        }

        return true;
    }
}
