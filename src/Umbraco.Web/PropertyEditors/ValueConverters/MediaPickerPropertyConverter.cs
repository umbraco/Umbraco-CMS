// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MediaPickerPropertyConverter.cs" company="Umbraco">
//   Umbraco
// </copyright>
// <summary>
//  The media picker 2 value converter
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;

namespace Umbraco.Web.PropertyEditors.ValueConverters
{
    /// <summary>
    /// The media picker property value converter.
    /// </summary>
    [DefaultPropertyValueConverter]
    public class MediaPickerPropertyConverter : PropertyValueConverterBase
    {
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;
        private readonly ServiceContext _services;
        private readonly CacheHelper _appCache;
        private readonly PropertyEditorCollection _propertyEditors;

        public MediaPickerPropertyConverter(ServiceContext services, PropertyEditorCollection propertyEditors, IUmbracoContextAccessor umbracoContextAccessor, CacheHelper appCache)
        {
            _umbracoContextAccessor = umbracoContextAccessor ?? throw new ArgumentNullException(nameof(umbracoContextAccessor));
            _appCache = appCache ?? throw new ArgumentNullException(nameof(appCache));
            _services = services ?? throw new ArgumentNullException(nameof(services));
            _propertyEditors = propertyEditors ?? throw new ArgumentException(nameof(propertyEditors));
        }

        public override bool IsConverter(PublishedPropertyType propertyType)
        {
            return propertyType.PropertyEditorAlias.Equals(Constants.PropertyEditors.MediaPicker2Alias);
        }

        public override Type GetPropertyValueType(PublishedPropertyType propertyType)
        {
            return IsMultipleDataType(propertyType.DataTypeId, propertyType.PropertyEditorAlias) 
                ? typeof(IEnumerable<IPublishedContent>) 
                : typeof(IPublishedContent);
        }

        public override PropertyCacheLevel GetPropertyCacheLevel(PublishedPropertyType propertyType) 
            => PropertyCacheLevel.Facade;

        private bool IsMultipleDataType(int dataTypeId, string propertyEditorAlias)
        {
            // GetPreValuesCollectionByDataTypeId is cached at repository level;
            // still, the collection is deep-cloned so this is kinda expensive,
            // better to cache here + trigger refresh in DataTypeCacheRefresher

            return Storages.GetOrAdd(dataTypeId, id =>
            {
                var preVals = _services.DataTypeService.GetPreValuesCollectionByDataTypeId(id).PreValuesAsDictionary;

                if (preVals.ContainsKey("multiPicker"))
                {
                    var preValue = preVals
                        .FirstOrDefault(x => string.Equals(x.Key, "multiPicker", StringComparison.InvariantCultureIgnoreCase))
                        .Value;

                    return preValue != null && preValue.Value.TryConvertTo<bool>().Result;
                }

                //in some odd cases, the pre-values in the db won't exist but their default pre-values contain this key so check there 
                if (_propertyEditors.TryGet(propertyEditorAlias, out PropertyEditor propertyEditor))
                {
                    var preValue = propertyEditor.DefaultPreValues
                        .FirstOrDefault(x => string.Equals(x.Key, "multiPicker", StringComparison.InvariantCultureIgnoreCase))
                        .Value;

                    return preValue != null && preValue.TryConvertTo<bool>().Result;
                }

                return false;
            });
        }

        public override object ConvertSourceToInter(PublishedPropertyType propertyType, object source, bool preview)
        {
            var nodeIds = source.ToString()
                .Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                .Select(Udi.Parse)
                .ToArray();
            return nodeIds;
        }

        public override object ConvertInterToObject(PublishedPropertyType propertyType, PropertyCacheLevel cacheLevel, object source, bool preview)
        {
            if (source == null)
            {
                return null;
            }

            var udis = (Udi[])source;
            var mediaItems = new List<IPublishedContent>();
            var helper = new UmbracoHelper(_umbracoContextAccessor.UmbracoContext, _services, _appCache);
            if (udis.Any())
            {
                foreach (var udi in udis)
                {
                    var item = helper.PublishedContent(udi);
                    if (item != null)
                        mediaItems.Add(item);
                }

                if (IsMultipleDataType(propertyType.DataTypeId, propertyType.PropertyEditorAlias))
                    return mediaItems;
                return mediaItems.FirstOrDefault();
            }

            return source;
        }

        private static readonly ConcurrentDictionary<int, bool> Storages = new ConcurrentDictionary<int, bool>();

        internal static void ClearCaches()
        {
            Storages.Clear();
        }
    }
}