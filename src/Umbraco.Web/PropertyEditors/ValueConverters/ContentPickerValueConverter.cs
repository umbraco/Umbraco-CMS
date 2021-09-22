using System;
using System.Collections.Generic;
using System.Globalization;
using Umbraco.Core;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;
using Umbraco.Web.PublishedCache;

namespace Umbraco.Web.PropertyEditors.ValueConverters
{
    internal class ContentPickerValueConverter : PropertyValueConverterBase
    {
        private readonly IPublishedSnapshotAccessor _publishedSnapshotAccessor;

        private static readonly List<string> PropertiesToExclude = new List<string>
        {
            Constants.Conventions.Content.InternalRedirectId.ToLower(CultureInfo.InvariantCulture),
            Constants.Conventions.Content.Redirect.ToLower(CultureInfo.InvariantCulture)
        };

        public ContentPickerValueConverter(IPublishedSnapshotAccessor publishedSnapshotAccessor)
        {
            _publishedSnapshotAccessor = publishedSnapshotAccessor;
        }

        public override bool IsConverter(IPublishedPropertyType propertyType)
            => propertyType.EditorAlias.Equals(Constants.PropertyEditors.Aliases.ContentPicker);

        public override Type GetPropertyValueType(IPublishedPropertyType propertyType)
            => typeof (IPublishedContent);

        public override PropertyCacheLevel GetPropertyCacheLevel(IPublishedPropertyType propertyType)
            => PropertyCacheLevel.Elements;

        public override object ConvertSourceToIntermediate(IPublishedElement owner, IPublishedPropertyType propertyType, object source, bool preview)
        {
            if (source == null) return null;

            //Don't attempt to convert to int for UDI
            if(!(source is string) || source is string strSource && !string.IsNullOrWhiteSpace(strSource) && !strSource.StartsWith("umb"))
            {
                 var attemptConvertInt = source.TryConvertTo<int>();
                    if (attemptConvertInt.Success)
                        return attemptConvertInt.Result;
            }

            var attemptConvertUdi = source.TryConvertTo<Udi>();
            if (attemptConvertUdi.Success)
                return attemptConvertUdi.Result;
            return null;
        }

        public override object ConvertIntermediateToObject(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object inter, bool preview)
        {
            if (inter == null)
                return null;

            if ((propertyType.Alias != null && PropertiesToExclude.Contains(propertyType.Alias.ToLower(CultureInfo.InvariantCulture))) == false)
            {
                IPublishedContent content;
                if (inter is int id)
                {
                    content = _publishedSnapshotAccessor.PublishedSnapshot.Content.GetById(id);
                    if (content != null)
                        return content;
                }
                else
                {
                    var udi = inter as GuidUdi;
                    if (udi == null)
                        return null;
                    content = _publishedSnapshotAccessor.PublishedSnapshot.Content.GetById(udi.Guid);
                    if (content != null && content.ContentType.ItemType == PublishedItemType.Content)
                        return content;
                }
            }

            return inter;
        }

        public override object ConvertIntermediateToXPath(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object inter, bool preview)
        {
            if (inter == null) return null;
            return inter.ToString();
        }
    }
}
