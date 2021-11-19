using System;
using Umbraco.Core;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;
using Umbraco.Web.Composing;
using Umbraco.Web.PublishedCache;
using Umbraco.Web.Security;

namespace Umbraco.Web.PropertyEditors.ValueConverters
{
    [DefaultPropertyValueConverter]
    public class MemberPickerValueConverter : PropertyValueConverterBase
    {
        private readonly IPublishedSnapshotAccessor _publishedSnapshotAccessor;

        public MemberPickerValueConverter(IPublishedSnapshotAccessor publishedSnapshotAccessor)
        {
            _publishedSnapshotAccessor = publishedSnapshotAccessor ?? throw new ArgumentNullException(nameof(publishedSnapshotAccessor));
        }

        public override bool IsConverter(IPublishedPropertyType propertyType)
        {
            return propertyType.EditorAlias.InvariantEquals(Constants.PropertyEditors.Aliases.MemberPicker);
        }

        public override PropertyCacheLevel GetPropertyCacheLevel(IPublishedPropertyType propertyType)
            => PropertyCacheLevel.Snapshot;

        public override Type GetPropertyValueType(IPublishedPropertyType propertyType)
            => typeof (IPublishedContent);

        public override object ConvertSourceToIntermediate(IPublishedElement owner, IPublishedPropertyType propertyType, object source, bool preview)
        {
            if (source == null) return null;

            var attemptConvertInt = source.TryConvertTo<int>();
            if (attemptConvertInt.Success)
                return attemptConvertInt.Result;
            var attemptConvertUdi = source.TryConvertTo<Udi>();
            if (attemptConvertUdi.Success)
                return attemptConvertUdi.Result;
            return null;
        }

        public override object ConvertIntermediateToObject(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel cacheLevel, object source, bool preview)
        {
            if (source == null)
                return null;

            if (Current.UmbracoContext != null)
            {
                IPublishedContent member;
                if (source is int id)
                {
                    member = _publishedSnapshotAccessor.PublishedSnapshot.Members.GetById(id);
                    if (member != null)
                        return member;
                }
                else
                {
                    var sourceUdi = source as GuidUdi;
                    if (sourceUdi == null) return null;
                    member = _publishedSnapshotAccessor.PublishedSnapshot.Members.GetByProviderKey(sourceUdi.Guid);
                    if (member != null)
                        return member;
                }
            }

            return source;
        }
    }
}
