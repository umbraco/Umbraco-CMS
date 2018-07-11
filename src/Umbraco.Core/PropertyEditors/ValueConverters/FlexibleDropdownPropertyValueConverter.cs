using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Services;

namespace Umbraco.Core.PropertyEditors.ValueConverters
{
    [DefaultPropertyValueConverter]
    public class FlexibleDropdownPropertyValueConverter : PropertyValueConverterBase //, IPropertyValueConverterMeta
    {
        private static readonly ConcurrentDictionary<int, bool> Storages = new ConcurrentDictionary<int, bool>();
        private readonly IDataTypeService _dataTypeService;

        public FlexibleDropdownPropertyValueConverter(IDataTypeService dataTypeService)
        {
            _dataTypeService = dataTypeService ?? throw new ArgumentNullException(nameof(dataTypeService));
        }

        public override bool IsConverter(PublishedPropertyType propertyType)
        {
            return propertyType.EditorAlias.Equals(Constants.PropertyEditors.Aliases.DropDownListFlexible);
        }

        public override object ConvertSourceToIntermediate(IPublishedElement owner, PublishedPropertyType propertyType, object source, bool preview)
        {
            return source != null
                       ? source.ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                       : null;
        }

        public override object ConvertIntermediateToObject(IPublishedElement owner, PublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object inter, bool preview)
        {
            if (inter == null)
            {
                return null;
            }

            var selectedValues = (string[])inter;
            if (selectedValues.Any())
            {
                if (IsMultipleDataType(propertyType.DataType.Id, propertyType.EditorAlias))
                {
                    return selectedValues;
                }

                return selectedValues.First();
            }

            return inter;
        }

        public override Type GetPropertyValueType(PublishedPropertyType propertyType)
        {
            return IsMultipleDataType(propertyType.DataType.Id, propertyType.EditorAlias)
                       ? typeof(IEnumerable<string>)
                       : typeof(string);
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
            //TODO: Fix this after this commit since we need to move this to the Web proje
            return false;

            //// GetPreValuesCollectionByDataTypeId is cached at repository level;
            //// still, the collection is deep-cloned so this is kinda expensive,
            //// better to cache here + trigger refresh in DataTypeCacheRefresher
            //return Storages.GetOrAdd(dataTypeId, id =>
            //{
            //    var dt = _dataTypeService.GetDataType(id);
            //    var preVals = (DropDownFlexibleConfigurationEditor)dt.Configuration;

            //    if (preVals.ContainsKey("multiple"))
            //    {
            //        var preValue = preVals
            //                       .FirstOrDefault(x => string.Equals(x.Key, "multiple",
            //                                                          StringComparison.InvariantCultureIgnoreCase))
            //                       .Value;

            //        return preValue != null && preValue.Value.TryConvertTo<bool>().Result;
            //    }

            //    //in some odd cases, the pre-values in the db won't exist but their default pre-values contain this key so check there 
            //    var propertyEditor = PropertyEditorResolver.Current.GetByAlias(propertyEditorAlias);
            //    if (propertyEditor != null)
            //    {
            //        var preValue = propertyEditor.DefaultPreValues
            //                                     .FirstOrDefault(x => string.Equals(x.Key, "multiple",
            //                                                                        StringComparison
            //                                                                            .InvariantCultureIgnoreCase))
            //                                     .Value;

            //        return preValue != null && preValue.TryConvertTo<bool>().Result;
            //    }

            //    return false;
            //});
        }

        internal static void ClearCaches()
        {
            Storages.Clear();
        }
    }
}
