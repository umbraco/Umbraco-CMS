using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.PropertyEditors.ValueConverters;
using Umbraco.Core.Services;

namespace Umbraco.Web.PropertyEditors.ValueConverters
{
    /// <summary>
    /// The multiple media picker property value converter.
    /// </summary>
    [DefaultPropertyValueConverter(typeof(MustBeStringValueConverter))]
    public class MediaPickerLegacyValueConverter : PropertyValueConverterBase
    {
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;
        private readonly ServiceContext _services;
        private readonly CacheHelper _appCache;
        private readonly PropertyEditorCollection _propertyEditors;
        private readonly ILogger _logger;

        public MediaPickerLegacyValueConverter(ServiceContext services, CacheHelper appCache, IUmbracoContextAccessor umbracoContextAccessor, PropertyEditorCollection propertyEditors, ILogger logger)
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));
            _appCache = appCache ?? throw new ArgumentNullException(nameof(appCache));
            _umbracoContextAccessor = umbracoContextAccessor ?? throw new ArgumentNullException(nameof(umbracoContextAccessor));
            _propertyEditors = propertyEditors ?? throw new ArgumentNullException(nameof(propertyEditors));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public override bool IsConverter(PublishedPropertyType propertyType)
        {
            return propertyType.PropertyEditorAlias.Equals(Constants.PropertyEditors.MultipleMediaPickerAlias);
        }

        public override PropertyCacheLevel GetPropertyCacheLevel(PublishedPropertyType propertyType)
            => PropertyCacheLevel.Facade;

        public override Type GetPropertyValueType(PublishedPropertyType propertyType)
            => IsMultipleDataType(propertyType.DataTypeId, propertyType.PropertyEditorAlias)
                ? typeof (IEnumerable<IPublishedContent>)
                : typeof (IPublishedContent);

        public override object ConvertSourceToInter(IPropertySet owner, PublishedPropertyType propertyType, object source, bool preview)
        {
            if (IsMultipleDataType(propertyType.DataTypeId, propertyType.PropertyEditorAlias))
            {
                return source.ToString()
                        .Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(int.Parse)
                        .ToArray();
            }

            var attemptConvertInt = source.TryConvertTo<int>();
            if (attemptConvertInt.Success)
            {
                return attemptConvertInt.Result;
            }

            var nodeIds =
                source.ToString()
                    .Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(int.Parse)
                    .ToArray();

            if (nodeIds.Length > 0)
            {
                var error = $"Data type \"{_services.DataTypeService.GetDataTypeDefinitionById(propertyType.DataTypeId).Name}\" is not set to allow multiple items but appears to contain multiple items, check the setting and save the data type again";

                _logger.Warn<MediaPickerLegacyValueConverter>(error);
                throw new Exception(error);
            }

            return null;
        }

        public override object ConvertInterToObject(IPropertySet owner, PublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object source, bool preview)
        {
            if (source == null)
            {
                return null;
            }

            if (UmbracoContext.Current == null)
            {
                return null;
            }

            var umbHelper = new UmbracoHelper(_umbracoContextAccessor.UmbracoContext, _services, _appCache);

            if (IsMultipleDataType(propertyType.DataTypeId, propertyType.PropertyEditorAlias))
            {
                var nodeIds = (int[])source;
                var multiMediaPicker = Enumerable.Empty<IPublishedContent>();
                if (nodeIds.Length > 0)
                {
                    multiMediaPicker =  umbHelper.Media(nodeIds).Where(x => x != null);
                }

                // in v8 should return multiNodeTreePickerEnumerable but for v7 need to return as PublishedContentEnumerable so that string can be returned for legacy compatibility
                return new PublishedContentEnumerable(multiMediaPicker);
            }

            // single value picker
            var nodeId = (int)source;

            return umbHelper.Media(nodeId);
        }

        /// <summary>
        /// The is multiple data type.
        /// </summary>
        /// <param name="dataTypeId">
        /// The data type id.
        /// </param>
        /// <param name="propertyEditorAlias"></param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
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

        private static readonly ConcurrentDictionary<int, bool> Storages = new ConcurrentDictionary<int, bool>();

        internal static void ClearCaches()
        {
            Storages.Clear();
        }
    }
}
