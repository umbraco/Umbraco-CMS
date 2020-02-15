using System;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Scoping;
using Umbraco.Infrastructure.Persistence.Repositories;

namespace Umbraco.Core.Services.Implement
{
    internal class KeyValueService : IKeyValueService
    {
        private readonly object _initialock = new object();
        private readonly IScopeProvider _scopeProvider;
        private readonly IKeyValueRepository _repository;
        private readonly ILogger _logger;
        private readonly IUmbracoVersion _umbracoVersion;
        private bool _initialized;

        public KeyValueService(IScopeProvider scopeProvider, IKeyValueRepository repository, ILogger logger, IUmbracoVersion umbracoVersion)
        {
            _scopeProvider = scopeProvider;
            _repository = repository;
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
                _repository.Initialize();
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
                return _repository.GetValue(key);
            }
        }

        /// <inheritdoc />
        public void SetValue(string key, string value)
        {
            EnsureInitialized();

            using (var scope = _scopeProvider.CreateScope())
            {
                scope.WriteLock(Constants.Locks.KeyValues);

                _repository.SetValue(key, value);

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

                if (!_repository.TrySetValue(key, originalValue, newValue))
                {
                    return false;
                }

                scope.Complete();
            }

            return true;
        }
    }
}
