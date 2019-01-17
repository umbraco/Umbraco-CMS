using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Services;

namespace Umbraco.Core.PropertyEditors.ValueConverters
{
    [DefaultPropertyValueConverter]
    public class FlexibleDropdownPropertyValueConverter : PropertyValueConverterBase, IPropertyValueConverterMeta
    {
        private static readonly ConcurrentDictionary<int, bool> Storages = new ConcurrentDictionary<int, bool>();
        private readonly IDataTypeService _dataTypeService;

        // TODO: Remove this ctor in v8 - the other one will be usable via IoC
        public FlexibleDropdownPropertyValueConverter() : this(ApplicationContext.Current.Services.DataTypeService)
        { }

        public FlexibleDropdownPropertyValueConverter(IDataTypeService dataTypeService)
        {
            Mandate.ParameterNotNull(dataTypeService, "dataTypeService");
            _dataTypeService = dataTypeService;
        }

        public override bool IsConverter(PublishedPropertyType propertyType)
        {
            return propertyType.PropertyEditorAlias.Equals(Constants.PropertyEditors.DropDownListFlexibleAlias);
        }

        public override object ConvertDataToSource(PublishedPropertyType propertyType, object source, bool preview)
        {
            return source != null
                       ? source.ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                       : null;
        }

        public override object ConvertSourceToObject(PublishedPropertyType propertyType, object source, bool preview)
        {
            if (source == null)
            {
                return null;
            }

            var selectedValues = (string[]) source;
            if (selectedValues.Any())
            {
                if (IsMultipleDataType(propertyType.DataTypeId, propertyType.PropertyEditorAlias))
                {
                    return selectedValues;
                }

                return selectedValues.First();
            }

            return source;
        }

        public Type GetPropertyValueType(PublishedPropertyType propertyType)
        {
            return IsMultipleDataType(propertyType.DataTypeId, propertyType.PropertyEditorAlias)
                       ? typeof(IEnumerable<string>)
                       : typeof(string);
        }

        public PropertyCacheLevel GetPropertyCacheLevel(PublishedPropertyType propertyType,
                                                        PropertyCacheValue cacheValue)
        {
            PropertyCacheLevel returnLevel;
            switch (cacheValue)
            {
                case PropertyCacheValue.Object:
                    returnLevel = PropertyCacheLevel.ContentCache;
                    break;
                case PropertyCacheValue.Source:
                    returnLevel = PropertyCacheLevel.Content;
                    break;
                case PropertyCacheValue.XPath:
                    returnLevel = PropertyCacheLevel.Content;
                    break;
                default:
                    returnLevel = PropertyCacheLevel.None;
                    break;
            }

            return returnLevel;
        }

        /// <summary>
        /// Determines if the "enable multiple choice" prevalue has been ticked.
        /// </summary>
        /// <param name="dataTypeId">The ID of this particular datatype instance.</param>
        /// <param name="propertyEditorAlias">The property editor alias.</param>
        /// <returns><value>true</value> if the data type has been configured to return multiple values.
        /// </returns>
        private bool IsMultipleDataType(int dataTypeId, string propertyEditorAlias)
        {
            // GetPreValuesCollectionByDataTypeId is cached at repository level;
            // still, the collection is deep-cloned so this is kinda expensive,
            // better to cache here + trigger refresh in DataTypeCacheRefresher
            return Storages.GetOrAdd(dataTypeId, id =>
            {
                var preVals = _dataTypeService.GetPreValuesCollectionByDataTypeId(id).PreValuesAsDictionary;

                if (preVals.ContainsKey("multiple"))
                {
                    var preValue = preVals
                                   .FirstOrDefault(x => string.Equals(x.Key, "multiple",
                                                                      StringComparison.InvariantCultureIgnoreCase))
                                   .Value;

                    return preValue != null && preValue.Value.TryConvertTo<bool>().Result;
                }

                //in some odd cases, the pre-values in the db won't exist but their default pre-values contain this key so check there 
                var propertyEditor = PropertyEditorResolver.Current.GetByAlias(propertyEditorAlias);
                if (propertyEditor != null)
                {
                    var preValue = propertyEditor.DefaultPreValues
                                                 .FirstOrDefault(x => string.Equals(x.Key, "multiple",
                                                                                    StringComparison
                                                                                        .InvariantCultureIgnoreCase))
                                                 .Value;

                    return preValue != null && preValue.TryConvertTo<bool>().Result;
                }

                return false;
            });
        }

        internal static void ClearCaches()
        {
            Storages.Clear();
        }
    }
}
