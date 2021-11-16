using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Services;

namespace Umbraco.Web.PublishedCache
{
    /// <summary>
    /// Represents a content type cache.
    /// </summary>
    /// <remarks>This cache is not snapshotted, so it refreshes any time things change.</remarks>
    public class PublishedContentTypeCache : IDisposable
    {
        // NOTE: These are not concurrent dictionaries because all access is done within a lock
        private readonly Dictionary<string, IPublishedContentType> _typesByAlias = new Dictionary<string, IPublishedContentType>();
        private readonly Dictionary<int, IPublishedContentType> _typesById = new Dictionary<int, IPublishedContentType>();
        private readonly Dictionary<Guid, int> _keyToIdMap = new Dictionary<Guid, int>();
        private readonly IContentTypeService _contentTypeService;
        private readonly IMediaTypeService _mediaTypeService;
        private readonly IMemberTypeService _memberTypeService;
        private readonly IPublishedContentTypeFactory _publishedContentTypeFactory;
        private readonly ILogger _logger;
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        // default ctor
        internal PublishedContentTypeCache(IContentTypeService contentTypeService, IMediaTypeService mediaTypeService, IMemberTypeService memberTypeService, IPublishedContentTypeFactory publishedContentTypeFactory, ILogger logger)
        {
            _contentTypeService = contentTypeService;
            _mediaTypeService = mediaTypeService;
            _memberTypeService = memberTypeService;
            _logger = logger;
            _publishedContentTypeFactory = publishedContentTypeFactory;
        }

        // for unit tests ONLY
        internal PublishedContentTypeCache(ILogger logger, IPublishedContentTypeFactory publishedContentTypeFactory)
        {
            _logger = logger;
            _publishedContentTypeFactory = publishedContentTypeFactory;
        }

        // note: cache clearing is performed by XmlStore

        /// <summary>
        /// Clears all cached content types.
        /// </summary>
        public void ClearAll()
        {
            _logger.Debug<PublishedContentTypeCache>("Clear all.");

            try
            {
                _lock.EnterWriteLock();

                _typesByAlias.Clear();
                _typesById.Clear();
            }
            finally
            {
                if (_lock.IsWriteLockHeld)
                    _lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Clears a cached content type.
        /// </summary>
        /// <param name="id">An identifier.</param>
        public void ClearContentType(int id)
        {
            _logger.Debug<PublishedContentTypeCache,int>("Clear content type w/id {ContentTypeId}", id);

            try
            {
                _lock.EnterUpgradeableReadLock();

                if (_typesById.TryGetValue(id, out var type) == false)
                    return;

                try
                {
                    _lock.EnterWriteLock();

                    _typesByAlias.Remove(GetAliasKey(type));
                    _typesById.Remove(id);
                }
                finally
                {
                    if (_lock.IsWriteLockHeld)
                        _lock.ExitWriteLock();
                }
            }
            finally
            {
                if (_lock.IsUpgradeableReadLockHeld)
                    _lock.ExitUpgradeableReadLock();
            }
        }

        /// <summary>
        /// Clears all cached content types referencing a data type.
        /// </summary>
        /// <param name="id">A data type identifier.</param>
        public void ClearDataType(int id)
        {
            _logger.Debug<PublishedContentTypeCache,int>("Clear data type w/id {DataTypeId}.", id);

            // there is no recursion to handle here because a PublishedContentType contains *all* its
            // properties ie both its own properties and those that were inherited (it's based upon an
            // IContentTypeComposition) and so every PublishedContentType having a property based upon
            // the cleared data type, be it local or inherited, will be cleared.

            try
            {
                _lock.EnterWriteLock();

                var toRemove = _typesById.Values.Where(x => x.PropertyTypes.Any(xx => xx.DataType.Id == id)).ToArray();
                foreach (var type in toRemove)
                {
                    _typesByAlias.Remove(GetAliasKey(type));
                    _typesById.Remove(type.Id);
                }
            }
            finally
            {
                if (_lock.IsWriteLockHeld)
                    _lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Gets a published content type.
        /// </summary>
        /// <param name="itemType">An item type.</param>
        /// <param name="key">An key.</param>
        /// <returns>The published content type corresponding to the item key.</returns>
        public IPublishedContentType Get(PublishedItemType itemType, Guid key)
        {
            try
            {
                _lock.EnterUpgradeableReadLock();

                if (_keyToIdMap.TryGetValue(key, out var id))
                    return Get(itemType, id);

                var type = CreatePublishedContentType(itemType, key);

                try
                {
                    _lock.EnterWriteLock();
                    _keyToIdMap[key] = type.Id;
                    return _typesByAlias[GetAliasKey(type)] = _typesById[type.Id] = type;
                }
                finally
                {
                    if (_lock.IsWriteLockHeld)
                        _lock.ExitWriteLock();
                }
            }
            finally
            {
                if (_lock.IsUpgradeableReadLockHeld)
                    _lock.ExitUpgradeableReadLock();
            }
        }

        /// <summary>
        /// Gets a published content type.
        /// </summary>
        /// <param name="itemType">An item type.</param>
        /// <param name="alias">An alias.</param>
        /// <returns>The published content type corresponding to the item type and alias.</returns>
        public IPublishedContentType Get(PublishedItemType itemType, string alias)
        {
            var aliasKey = GetAliasKey(itemType, alias);

            try
            {
                _lock.EnterUpgradeableReadLock();

                if (_typesByAlias.TryGetValue(aliasKey, out var type))
                    return type;

                type = CreatePublishedContentType(itemType, alias);

                try
                {
                    _lock.EnterWriteLock();
                    if (type.TryGetKey(out var key))
                        _keyToIdMap[key] = type.Id;
                    return _typesByAlias[aliasKey] = _typesById[type.Id] = type;
                }
                finally
                {
                    if (_lock.IsWriteLockHeld)
                        _lock.ExitWriteLock();
                }
            }
            finally
            {
                if (_lock.IsUpgradeableReadLockHeld)
                    _lock.ExitUpgradeableReadLock();
            }
        }

        /// <summary>
        /// Gets a published content type.
        /// </summary>
        /// <param name="itemType">An item type.</param>
        /// <param name="id">An identifier.</param>
        /// <returns>The published content type corresponding to the item type and identifier.</returns>
        public IPublishedContentType Get(PublishedItemType itemType, int id)
        {
            try
            {
                _lock.EnterUpgradeableReadLock();

                if (_typesById.TryGetValue(id, out var type))
                    return type;

                type = CreatePublishedContentType(itemType, id);

                try
                {
                    _lock.EnterWriteLock();
                    if (type.TryGetKey(out var key))
                        _keyToIdMap[key] = type.Id;
                    return _typesByAlias[GetAliasKey(type)] = _typesById[type.Id] = type;
                }
                finally
                {
                    if (_lock.IsWriteLockHeld)
                        _lock.ExitWriteLock();
                }
            }
            finally
            {
                if (_lock.IsUpgradeableReadLockHeld)
                    _lock.ExitUpgradeableReadLock();
            }
        }

        private IPublishedContentType CreatePublishedContentType(PublishedItemType itemType, Guid key)
        {
            IContentTypeComposition contentType = itemType switch
            {
                PublishedItemType.Content => _contentTypeService.Get(key),
                PublishedItemType.Media => _mediaTypeService.Get(key),
                PublishedItemType.Member => _memberTypeService.Get(key),
                _ => throw new ArgumentOutOfRangeException(nameof(itemType)),
            };
            if (contentType == null)
                throw new Exception($"ContentTypeService failed to find a {itemType.ToString().ToLower()} type with key \"{key}\".");

            return _publishedContentTypeFactory.CreateContentType(contentType);
        }

        private IPublishedContentType CreatePublishedContentType(PublishedItemType itemType, string alias)
        {
            if (GetPublishedContentTypeByAlias != null)
                return GetPublishedContentTypeByAlias(alias);
            IContentTypeComposition contentType = itemType switch
            {
                PublishedItemType.Content => _contentTypeService.Get(alias),
                PublishedItemType.Media => _mediaTypeService.Get(alias),
                PublishedItemType.Member => _memberTypeService.Get(alias),
                _ => throw new ArgumentOutOfRangeException(nameof(itemType)),
            };
            if (contentType == null)
                throw new Exception($"ContentTypeService failed to find a {itemType.ToString().ToLower()} type with alias \"{alias}\".");

            return _publishedContentTypeFactory.CreateContentType(contentType);
        }

        private IPublishedContentType CreatePublishedContentType(PublishedItemType itemType, int id)
        {
            if (GetPublishedContentTypeById != null)
                return GetPublishedContentTypeById(id);
            IContentTypeComposition contentType = itemType switch
            {
                PublishedItemType.Content => _contentTypeService.Get(id),
                PublishedItemType.Media => _mediaTypeService.Get(id),
                PublishedItemType.Member => _memberTypeService.Get(id),
                _ => throw new ArgumentOutOfRangeException(nameof(itemType)),
            };
            if (contentType == null)
                throw new Exception($"ContentTypeService failed to find a {itemType.ToString().ToLower()} type with id {id}.");

            return _publishedContentTypeFactory.CreateContentType(contentType);
        }

        // for unit tests - changing the callback must reset the cache obviously
        // TODO: Why does this even exist? For testing you'd pass in a mocked service to get by id
        private Func<string, IPublishedContentType> _getPublishedContentTypeByAlias;
        internal Func<string, IPublishedContentType> GetPublishedContentTypeByAlias
        {
            get => _getPublishedContentTypeByAlias;
            set
            {
                try
                {
                    _lock.EnterWriteLock();

                    _typesByAlias.Clear();
                    _typesById.Clear();
                    _getPublishedContentTypeByAlias = value;
                }
                finally
                {
                    if (_lock.IsWriteLockHeld)
                        _lock.ExitWriteLock();
                }
            }
        }

        // for unit tests - changing the callback must reset the cache obviously
        // TODO: Why does this even exist? For testing you'd pass in a mocked service to get by id
        private Func<int, IPublishedContentType> _getPublishedContentTypeById;
        private bool _disposedValue;

        internal Func<int, IPublishedContentType> GetPublishedContentTypeById
        {
            get => _getPublishedContentTypeById;
            set
            {
                try
                {
                    _lock.EnterWriteLock();

                    _typesByAlias.Clear();
                    _typesById.Clear();
                    _getPublishedContentTypeById = value;
                }
                finally
                {
                    if (_lock.IsWriteLockHeld)
                        _lock.ExitWriteLock();
                }
            }
        }

        private static string GetAliasKey(PublishedItemType itemType, string alias)
        {
            string k;

            switch (itemType)
            {
                case PublishedItemType.Content:
                    k = "c";
                    break;
                case PublishedItemType.Media:
                    k = "m";
                    break;
                case PublishedItemType.Member:
                    k = "m";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(itemType));
            }

            return k + ":" + alias;
        }

        private static string GetAliasKey(IPublishedContentType contentType)
        {
            return GetAliasKey(contentType.ItemType, contentType.Alias);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _lock.Dispose();
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
        }
    }
}
