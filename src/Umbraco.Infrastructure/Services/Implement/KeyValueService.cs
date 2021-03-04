﻿using System;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;

namespace Umbraco.Cms.Core.Services.Implement
{
    internal class KeyValueService : IKeyValueService
    {
        private readonly IScopeProvider _scopeProvider;
        private readonly IKeyValueRepository _repository;

        public KeyValueService(IScopeProvider scopeProvider, IKeyValueRepository repository)
        {
            _scopeProvider = scopeProvider;
            _repository = repository;
        }

        /// <inheritdoc />
        public string GetValue(string key)
        {
            using (var scope = _scopeProvider.CreateScope(autoComplete: true))
            {
                return _repository.Get(key)?.Value;
            }
        }

        /// <inheritdoc />
        public void SetValue(string key, string value)
        {
            using (var scope = _scopeProvider.CreateScope())
            {
                scope.WriteLock(Cms.Core.Constants.Locks.KeyValues);

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
            using (var scope = _scopeProvider.CreateScope())
            {
                scope.WriteLock(Cms.Core.Constants.Locks.KeyValues);

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
