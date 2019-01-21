using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;
using Umbraco.Web.Models;
using Umbraco.Web.PublishedCache;

namespace Umbraco.Web.PropertyEditors.ValueConverters
{
    public class MultiUrlPickerValueConverter : PropertyValueConverterBase
    {
        private readonly IPublishedSnapshotAccessor _publishedSnapshotAccessor;
        private readonly IProfilingLogger _proflog;

        public MultiUrlPickerValueConverter(IPublishedSnapshotAccessor publishedSnapshotAccessor, IProfilingLogger proflog)
        {
            _publishedSnapshotAccessor = publishedSnapshotAccessor ?? throw new ArgumentNullException(nameof(publishedSnapshotAccessor));
            _proflog = proflog ?? throw new ArgumentNullException(nameof(proflog));
        }

        public override bool IsConverter(PublishedPropertyType propertyType) => Constants.PropertyEditors.Aliases.MultiUrlPicker.Equals(propertyType.EditorAlias);

        public override Type GetPropertyValueType(PublishedPropertyType propertyType) =>
            propertyType.DataType.ConfigurationAs<MultiUrlPickerConfiguration>().MaxNumber == 1 ?
                typeof(Link) :
                typeof(IEnumerable<Link>);

        public override PropertyCacheLevel GetPropertyCacheLevel(PublishedPropertyType propertyType) => PropertyCacheLevel.Snapshot;

        public override bool? IsValue(object value, PropertyValueLevel level) => value?.ToString() != "[]";

        public override object ConvertSourceToIntermediate(IPublishedElement owner, PublishedPropertyType propertyType, object source, bool preview) => source?.ToString();

        public override object ConvertIntermediateToObject(IPublishedElement owner, PublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object inter, bool preview)
        {
            using (_proflog.DebugDuration<MultiUrlPickerValueConverter>($"ConvertPropertyToLinks ({propertyType.DataType.Id})"))
            {
                var maxNumber = propertyType.DataType.ConfigurationAs<MultiUrlPickerConfiguration>().MaxNumber;

                if (inter == null)
                {
                    return maxNumber == 1 ? null : Enumerable.Empty<Link>();
                }

                var links = new List<Link>();
                var dtos = JsonConvert.DeserializeObject<IEnumerable<MultiUrlPickerValueEditor.LinkDto>>(inter.ToString());

                foreach (var dto in dtos)
                {
                    var type = LinkType.External;
                    var url = dto.Url;

                    if (dto.Udi != null)
                    {
                        type = dto.Udi.EntityType == Core.Constants.UdiEntityType.Media
                            ? LinkType.Media
                            : LinkType.Content;

                        var content = type == LinkType.Media ?
                             _publishedSnapshotAccessor.PublishedSnapshot.Media.GetById(preview, dto.Udi.Guid) :
                             _publishedSnapshotAccessor.PublishedSnapshot.Content.GetById(preview, dto.Udi.Guid);

                        if (content == null)
                        {
                            continue;
                        }
                        url = content.Url;
                    }

                    links.Add(
                        new Link
                        {
                            Name = dto.Name,
                            Target = dto.Target,
                            Type = type,
                            Udi = dto.Udi,
                            Url = url,
                        }
                    );
                }

                if (maxNumber == 1) return links.FirstOrDefault();
                if (maxNumber > 0) return links.Take(maxNumber);
                return links;
            }
        }
    }
}
