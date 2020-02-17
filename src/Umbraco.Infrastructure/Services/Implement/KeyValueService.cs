using System;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Scoping;
using Umbraco.Infrastructure.Migrations.Custom;

namespace Umbraco.Core.Services.Implement
{
    internal class KeyValueService : IKeyValueService
    {
        private readonly object _initialock = new object();
        private readonly IScopeProvider _scopeProvider;
        private readonly IKeyValueRepository _repository;
        private readonly IKeyValueServiceInitialization _initialization;
        private readonly ILogger _logger;
        private readonly IUmbracoVersion _umbracoVersion;
        private bool _initialized;

        public KeyValueService(IScopeProvider scopeProvider, IKeyValueRepository repository, IKeyValueServiceInitialization initialization, ILogger logger, IUmbracoVersion umbracoVersion)
        {
            _scopeProvider = scopeProvider;
            _repository = repository;
            _initialization = initialization;
            _logger = logger;
            _umbracoVersion = umbracoVersion;
        }

        private void EnsureInitialized()
        {
            lock (_initialock)
            {
                if (_initialized) return;
                Initialize();
            }
        }

        private void Initialize()
        {
            // the key/value service is entirely self-managed, because it is used by the
            // upgrader and anything we might change need to happen before everything else

            // if already running 8, either following an upgrade or an install,
            // then everything should be ok (the table should exist, etc)

            if (_umbracoVersion.LocalVersion != null && _umbracoVersion.LocalVersion.Major >= 8)
            {
                _initialized = true;
                return;
            }

            // else we are upgrading from 7, we can assume that the locks table
            // exists, but we need to create everything for key/value

            using (var scope = _scopeProvider.CreateScope())
            {
                _initialization.PerformInitialMigration(scope.Database);
                scope.Complete();
            }

            // but don't assume we are initializing
            // we are upgrading from v7 and if anything goes wrong,
            // the table and everything will be rolled back
        }        

        /// <inheritdoc />
        public string GetValue(string key)
        {
            EnsureInitialized();

            using (var scope = _scopeProvider.CreateScope())
            {
                return _repository.Get(key)?.Value;
            }
        }

        /// <inheritdoc />
        public void SetValue(string key, string value)
        {
            EnsureInitialized();

            using (var scope = _scopeProvider.CreateScope())
            {
                scope.WriteLock(Constants.Locks.KeyValues);

                var keyValue = _repository.Get(key);
                if (keyValue == null)
                {
                    keyValue = new KeyValue
                    {
                        Identifier = key,
                        Value = value,
                        UpdateDate = DateTime.Now,
                    };
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
                throw new InvalidOperationException("Could not set the value.");
        }

        /// <inheritdoc />
        public bool TrySetValue(string key, string originalValue, string newValue)
        {
            EnsureInitialized();

            using (var scope = _scopeProvider.CreateScope())
            {
                scope.WriteLock(Constants.Locks.KeyValues);

                var keyValue = _repository.Get(key);
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
}
