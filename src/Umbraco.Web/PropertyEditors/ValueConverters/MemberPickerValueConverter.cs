using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        {
            var isMultiple = IsMultipleDataType(propertyType.DataType);
            return isMultiple
                    ? typeof(IEnumerable<IPublishedContent>)
                    : typeof(IPublishedContent);
        }

        private bool IsMultipleDataType(PublishedDataType dataType)
        {
            return ConfigurationEditor.ConfigurationAs<MemberPickerConfiguration>(dataType.Configuration).MaxNumber == 1;
        }

        public override object ConvertSourceToIntermediate(IPublishedElement owner, IPublishedPropertyType propertyType, object source, bool preview)
        {
            if (source == null) return null;

            var nodeIds = source.ToString()
                .Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                .Select(Udi.Parse)
                .ToArray();

            return nodeIds;
        }

        public override object ConvertIntermediateToObject(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel cacheLevel, object source, bool preview)
        {
            var isMultiple = IsMultipleDataType(propertyType.DataType);

            var udis = (Udi[])source;
            var memberItems = new List<IPublishedContent>();

            if (source == null) return isMultiple ? memberItems : null;

            if (udis.Any())
            {
                foreach (var udi in udis)
                {
                    var guidUdi = udi as GuidUdi;
                    if (guidUdi == null) continue;
                    var item = _publishedSnapshotAccessor.PublishedSnapshot.Members.GetById(guidUdi.Guid);
                    if (item != null)
                        memberItems.Add(item);
                }

                return isMultiple ? memberItems : FirstOrDefault(memberItems);
            }

            return source;
        }

        private object FirstOrDefault(IList memberItems)
        {
            return memberItems.Count == 0 ? null : memberItems[0];
        }
    }
}
