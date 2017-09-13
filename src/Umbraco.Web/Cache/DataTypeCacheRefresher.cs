using System;
using Umbraco.Core.Cache;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors.ValueConverters;
using Umbraco.Core.Services;
using Umbraco.Web.PropertyEditors;
using Umbraco.Web.PropertyEditors.ValueConverters;
using Umbraco.Web.PublishedCache;


namespace Umbraco.Web.Cache
{
    public sealed class DataTypeCacheRefresher : PayloadCacheRefresherBase<DataTypeCacheRefresher, DataTypeCacheRefresher.JsonPayload>
    {
        private readonly IFacadeService _facadeService;
        private readonly IdkMap _idkMap;

        public DataTypeCacheRefresher(CacheHelper cacheHelper, IFacadeService facadeService, IdkMap idkMap)
            : base(cacheHelper)
        {
            _facadeService = facadeService;
            _idkMap = idkMap;
        }

        #region Define

        protected override DataTypeCacheRefresher This => this;

        public static readonly Guid UniqueId = Guid.Parse("35B16C25-A17E-45D7-BC8F-EDAB1DCC28D2");

        public override Guid RefresherUniqueId => UniqueId;

        public override string Name => "Data Type Cache Refresher";

        #endregion

        #region Refresher

        public override void Refresh(JsonPayload[] payloads)
        {
            //we need to clear the ContentType runtime cache since that is what caches the
            // db data type to store the value against and anytime a datatype changes, this also might change
            // we basically need to clear all sorts of runtime caches here because so many things depend upon a data type

            ClearAllIsolatedCacheByEntityType<IContent>();
            ClearAllIsolatedCacheByEntityType<IContentType>();
            ClearAllIsolatedCacheByEntityType<IMedia>();
            ClearAllIsolatedCacheByEntityType<IMediaType>();
            ClearAllIsolatedCacheByEntityType<IMember>();
            ClearAllIsolatedCacheByEntityType<IMemberType>();

            var dataTypeCache = CacheHelper.IsolatedRuntimeCache.GetCache<IDataTypeDefinition>();

            foreach (var payload in payloads)
            {
                if (dataTypeCache)
                    dataTypeCache.Result.ClearCacheByKeySearch(CacheKeys.DataTypePreValuesCacheKey + "_" + payload.Id);

                _idkMap.ClearCache(payload.Id);
                NestedContentHelper.ClearCache(payload.Id); // fixme refactor nested content
            }

            // fixme - not sure I like these?
            TagsValueConverter.ClearCaches();
            MediaPickerLegacyValueConverter.ClearCaches();
            SliderValueConverter.ClearCaches();
            MediaPickerValueConverter.ClearCaches();

            // notify
            _facadeService.Notify(payloads);

            base.Refresh(payloads);
        }

        // these events should never trigger
        // everything should be PAYLOAD/JSON

        public override void RefreshAll()
        {
            throw new NotSupportedException();
        }

        public override void Refresh(int id)
        {
            throw new NotSupportedException();
        }

        public override void Refresh(Guid id)
        {
            throw new NotSupportedException();
        }

        public override void Remove(int id)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region Json

        public class JsonPayload
        {
            public JsonPayload(int id, Guid key, bool removed)
            {
                Id = id;
                Key = key;
                Removed = removed;
            }

            public int Id { get; }

            public Guid Key { get; }

            public bool Removed { get; }
        }

        #endregion
    }
}
