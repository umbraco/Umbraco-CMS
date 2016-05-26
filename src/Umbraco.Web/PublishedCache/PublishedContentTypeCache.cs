using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Services;

namespace Umbraco.Web.PublishedCache
{
    // caches content, media and member types

    public class PublishedContentTypeCache
    {
        private readonly Dictionary<string, PublishedContentType> _typesByAlias = new Dictionary<string, PublishedContentType>();
        private readonly Dictionary<int, PublishedContentType> _typesById = new Dictionary<int, PublishedContentType>();
        private readonly IContentTypeService _contentTypeService;
        private readonly IMediaTypeService _mediaTypeService;
        private readonly IMemberTypeService _memberTypeService;
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        internal PublishedContentTypeCache(IContentTypeService contentTypeService, IMediaTypeService mediaTypeService, IMemberTypeService memberTypeService)
        {
            _contentTypeService = contentTypeService;
            _mediaTypeService = mediaTypeService;
            _memberTypeService = memberTypeService;
        }

        // for unit tests ONLY
        internal PublishedContentTypeCache()
        { }

        public void ClearAll()
        {
            Core.Logging.LogHelper.Debug<PublishedContentTypeCache>("Clear all.");

            using (new WriteLock(_lock))
            {
                _typesByAlias.Clear();
                _typesById.Clear();
            }
        }

        public void ClearContentType(int id)
        {
            Core.Logging.LogHelper.Debug<PublishedContentTypeCache>("Clear content type w/id {0}.", () => id);

            using (var l = new UpgradeableReadLock(_lock))
            {
                PublishedContentType type;
                if (_typesById.TryGetValue(id, out type) == false)
                    return;

                l.UpgradeToWriteLock();

                _typesByAlias.Remove(GetAliasKey(type));
                _typesById.Remove(id);
            }
        }

        public void ClearDataType(int id)
        {
            Core.Logging.LogHelper.Debug<PublishedContentTypeCache>("Clear data type w/id {0}.", () => id);

            // there is no recursion to handle here because a PublishedContentType contains *all* its
            // properties ie both its own properties and those that were inherited (it's based upon an
            // IContentTypeComposition) and so every PublishedContentType having a property based upon
            // the cleared data type, be it local or inherited, will be cleared.

            using (new WriteLock(_lock))
            {
                var toRemove = _typesById.Values.Where(x => x.PropertyTypes.Any(xx => xx.DataTypeId == id)).ToArray();
                foreach (var type in toRemove)
                {
                    _typesByAlias.Remove(GetAliasKey(type));
                    _typesById.Remove(type.Id);
                }
            }
        }

        public PublishedContentType Get(PublishedItemType itemType, string alias)
        {
            var aliasKey = GetAliasKey(itemType, alias);
            using (var l = new UpgradeableReadLock(_lock))
            {
                PublishedContentType type;
                if (_typesByAlias.TryGetValue(aliasKey, out type))
                    return type;
                type = CreatePublishedContentType(itemType, alias);
                l.UpgradeToWriteLock();
                return _typesByAlias[aliasKey] = _typesById[type.Id] = type;
            }
        }

        public PublishedContentType Get(PublishedItemType itemType, int id)
        {
            using (var l = new UpgradeableReadLock(_lock))
            {
                PublishedContentType type;
                if (_typesById.TryGetValue(id, out type))
                    return type;
                type = CreatePublishedContentType(itemType, id);
                l.UpgradeToWriteLock();
                return _typesByAlias[GetAliasKey(type)] = _typesById[type.Id] = type;
            }
        }

        private PublishedContentType CreatePublishedContentType(PublishedItemType itemType, string alias)
        {
            if (GetPublishedContentTypeByAlias != null)
                return GetPublishedContentTypeByAlias(alias);

            IContentTypeComposition contentType;
            switch (itemType)
            {
                case PublishedItemType.Content:
                    contentType = _contentTypeService.Get(alias);
                    break;
                case PublishedItemType.Media:
                    contentType = _mediaTypeService.Get(alias);
                    break;
                case PublishedItemType.Member:
                    contentType = _memberTypeService.Get(alias);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(itemType));
            }

            if (contentType == null)
                throw new Exception($"ContentTypeService failed to find a {itemType.ToString().ToLower()} type with alias \"{alias}\".");

            return new PublishedContentType(itemType, contentType);
        }

        private PublishedContentType CreatePublishedContentType(PublishedItemType itemType, int id)
        {
            if (GetPublishedContentTypeById != null)
                return GetPublishedContentTypeById(id);

            IContentTypeComposition contentType;
            switch (itemType)
            {
                case PublishedItemType.Content:
                    contentType = _contentTypeService.Get(id);
                    break;
                case PublishedItemType.Media:
                    contentType = _mediaTypeService.Get(id);
                    break;
                case PublishedItemType.Member:
                    contentType = _memberTypeService.Get(id);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(itemType));
            }

            if (contentType == null)
                throw new Exception($"ContentTypeService failed to find a {itemType.ToString().ToLower()} type with id {id}.");

            return new PublishedContentType(itemType, contentType);
        }

        // for unit tests - changing the callback must reset the cache obviously
        private Func<string, PublishedContentType> _getPublishedContentTypeByAlias;
        internal Func<string, PublishedContentType> GetPublishedContentTypeByAlias
        {
            get { return _getPublishedContentTypeByAlias; }
            set
            {
                using (new WriteLock(_lock))
                {
                    _typesByAlias.Clear();
                    _typesById.Clear();
                    _getPublishedContentTypeByAlias = value;
                }
            }
        }

        // for unit tests - changing the callback must reset the cache obviously
        private Func<int, PublishedContentType> _getPublishedContentTypeById;
        internal Func<int, PublishedContentType> GetPublishedContentTypeById
        {
            get { return _getPublishedContentTypeById; }
            set
            {
                using (new WriteLock(_lock))
                {
                    _typesByAlias.Clear();
                    _typesById.Clear();
                    _getPublishedContentTypeById = value;
                }
            }
        }

        private static string GetAliasKey(PublishedItemType itemType, string alias)
        {
            string k;

            if (itemType == PublishedItemType.Content)
                k = "c";
            else if (itemType == PublishedItemType.Media)
                k = "m";
            else if (itemType == PublishedItemType.Member)
                k = "m";
            else throw new ArgumentOutOfRangeException(nameof(itemType));

            return k + ":" + alias;
        }

        private static string GetAliasKey(PublishedContentType contentType)
        {
            return GetAliasKey(contentType.ItemType, contentType.Alias);
        }
    }
}
