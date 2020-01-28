using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
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
        private readonly IProfilingLogger _profilingLogger;

        public MultiUrlPickerValueConverter(IPublishedSnapshotAccessor publishedSnapshotAccessor, IProfilingLogger profilingLogger)
        {
            _publishedSnapshotAccessor = publishedSnapshotAccessor ?? throw new ArgumentNullException(nameof(publishedSnapshotAccessor));
            _profilingLogger = profilingLogger ?? throw new ArgumentNullException(nameof(profilingLogger));
        }

        public override bool IsConverter(IPublishedPropertyType propertyType) => Constants.PropertyEditors.Aliases.MultiUrlPicker.Equals(propertyType.EditorAlias);

        public override Type GetPropertyValueType(IPublishedPropertyType propertyType) => propertyType.DataType.ConfigurationAs<MultiUrlPickerConfiguration>().MaxNumber == 1
            ? typeof(Link)
            : typeof(IEnumerable<Link>);

        public override PropertyCacheLevel GetPropertyCacheLevel(IPublishedPropertyType propertyType) => PropertyCacheLevel.Snapshot;

        public override bool? IsValue(object value, PropertyValueLevel level) => value?.ToString() is var jsonValue && !string.IsNullOrEmpty(jsonValue) && jsonValue != "[]";

        public override object ConvertSourceToIntermediate(IPublishedElement owner, IPublishedPropertyType propertyType, object source, bool preview) => source?.ToString();

        public override object ConvertIntermediateToObject(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object inter, bool preview)
        {
            using (_profilingLogger.DebugDuration<MultiUrlPickerValueConverter>($"ConvertPropertyToLinks ({propertyType.DataType.Id})"))
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
                    IPublishedContent content = null;

                    if (dto.Udi != null)
                    {
                        switch (dto.Udi.EntityType)
                        {
                            case Core.Constants.UdiEntityType.Document:
                                type = LinkType.Content;
                                content = _publishedSnapshotAccessor.PublishedSnapshot.Content.GetById(preview, dto.Udi.Guid);
                                break;
                            case Core.Constants.UdiEntityType.Media:
                                type = LinkType.Media;
                                content = _publishedSnapshotAccessor.PublishedSnapshot.Media.GetById(preview, dto.Udi.Guid);
                                break;
                            default:
                                // Skip unsupported entity types
                                continue;
                        }

                        if (content == null || content.ContentType.ItemType == PublishedItemType.Element)
                        {
                            // Skip unavailable content and elements (in case the type has changed)
                            continue;
                        }

                        url = content.Url();
                    }

                    links.Add(new Link
                    {
                        Name = dto.Name,
                        Target = dto.Target,
                        Type = type,
                        Udi = dto.Udi,
                        Content = content,
                        Url = url + dto.QueryString,
                    });
                }

                if (maxNumber == 1) return links.FirstOrDefault();
                if (maxNumber > 0) return links.Take(maxNumber);
                return links;
            }
        }
    }
}
