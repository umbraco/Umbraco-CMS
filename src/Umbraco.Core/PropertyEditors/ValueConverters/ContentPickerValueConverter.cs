using System;
using System.Collections.Generic;
using System.Globalization;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters
{
    internal class ContentPickerValueConverter : PropertyValueConverterBase
    {
        private readonly IPublishedSnapshotAccessor _publishedSnapshotAccessor;

        private static readonly List<string> PropertiesToExclude = new List<string>
        {
            Constants.Conventions.Content.InternalRedirectId.ToLower(CultureInfo.InvariantCulture),
            Constants.Conventions.Content.Redirect.ToLower(CultureInfo.InvariantCulture)
        };

        public ContentPickerValueConverter(IPublishedSnapshotAccessor publishedSnapshotAccessor) => _publishedSnapshotAccessor = publishedSnapshotAccessor;

        public override bool IsConverter(IPublishedPropertyType propertyType)
            => propertyType.EditorAlias.Equals(Constants.PropertyEditors.Aliases.ContentPicker);

        public override Type GetPropertyValueType(IPublishedPropertyType propertyType)
            => typeof(IPublishedContent);

        public override PropertyCacheLevel GetPropertyCacheLevel(IPublishedPropertyType propertyType)
            => PropertyCacheLevel.Elements;

        public override object? ConvertSourceToIntermediate(IPublishedElement owner, IPublishedPropertyType propertyType, object? source, bool preview)
        {
            if (source == null) return null;


            if(source is not string)
            {
                var attemptConvertInt = source.TryConvertTo<int>();
                if (attemptConvertInt.Success)
                    return attemptConvertInt.Result;
            }
            //Don't attempt to convert to int for UDI
            if( source is string strSource
                && !string.IsNullOrWhiteSpace(strSource)
                && !strSource.StartsWith("umb")
                && int.TryParse(strSource, NumberStyles.Integer, CultureInfo.InvariantCulture, out var intValue))
            {
                return intValue;
            }

            var attemptConvertUdi = source.TryConvertTo<Udi>();
            if (attemptConvertUdi.Success)
                return attemptConvertUdi.Result;
            return null;
        }

        public override object? ConvertIntermediateToObject(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object? inter, bool preview)
        {
            if (inter == null)
                return null;

            if ((propertyType.Alias != null && PropertiesToExclude.Contains(propertyType.Alias.ToLower(CultureInfo.InvariantCulture))) == false)
            {
                IPublishedContent? content;
                var publishedSnapshot = _publishedSnapshotAccessor.GetRequiredPublishedSnapshot();
                if (inter is int id)
                {
                    content = publishedSnapshot.Content?.GetById(id);
                    if (content != null)
                        return content;
                }
                else
                {
                    var udi = inter as GuidUdi;
                    if (udi is null)
                        return null;
                    content = publishedSnapshot.Content?.GetById(udi.Guid);
                    if (content != null && content.ContentType.ItemType == PublishedItemType.Content)
                        return content;
                }
            }

            return inter;
        }

        public override object? ConvertIntermediateToXPath(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object? inter, bool preview)
        {
            if (inter == null) return null;
            return inter.ToString();
        }
    }
}
