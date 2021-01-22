using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.PropertyEditors.ValueConverters;
using Umbraco.Core.Services;
using Umbraco.Web.Composing;
using Umbraco.Web.PublishedCache;

namespace Umbraco.Web.PropertyEditors.ValueConverters
{

    /// <summary>
    /// The multi node tree picker property editor value converter.
    /// </summary>
    [DefaultPropertyValueConverter(typeof(MustBeStringValueConverter))]
    public class MultiNodeTreePickerValueConverter : PropertyValueConverterBase
    {
        private readonly IPublishedSnapshotAccessor _publishedSnapshotAccessor;

        private static readonly List<string> PropertiesToExclude = new List<string>
        {
            Constants.Conventions.Content.InternalRedirectId.ToLower(CultureInfo.InvariantCulture),
            Constants.Conventions.Content.Redirect.ToLower(CultureInfo.InvariantCulture)
        };

        public MultiNodeTreePickerValueConverter(IPublishedSnapshotAccessor publishedSnapshotAccessor)
        {
            _publishedSnapshotAccessor = publishedSnapshotAccessor ?? throw new ArgumentNullException(nameof(publishedSnapshotAccessor));
        }

        public override bool IsConverter(IPublishedPropertyType propertyType)
        {
            return propertyType.EditorAlias.Equals(Constants.PropertyEditors.Aliases.MultiNodeTreePicker);
        }

        public override PropertyCacheLevel GetPropertyCacheLevel(IPublishedPropertyType propertyType)
            => PropertyCacheLevel.Snapshot;

        public override Type GetPropertyValueType(IPublishedPropertyType propertyType)
            => IsSingleNodePicker(propertyType)
                ? typeof(IPublishedContent)
                : typeof(IEnumerable<IPublishedContent>);

        public override object ConvertSourceToIntermediate(IPublishedElement owner, IPublishedPropertyType propertyType, object source, bool preview)
        {
            if (source == null) return null;

            if (propertyType.EditorAlias.Equals(Constants.PropertyEditors.Aliases.MultiNodeTreePicker))
            {
                var nodeIds = source.ToString()
                    .Split(Constants.CharArrays.Comma, StringSplitOptions.RemoveEmptyEntries)
                    .Select(Udi.Parse)
                    .ToArray();
                return nodeIds;
            }
            return null;
        }

        public override object ConvertIntermediateToObject(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel cacheLevel, object source, bool preview)
        {
            if (source == null)
            {
                return null;
            }

            // TODO: Inject an UmbracoHelper and create a GetUmbracoHelper method based on either injected or singleton
            if (Current.UmbracoContext != null)
            {
                if (propertyType.EditorAlias.Equals(Constants.PropertyEditors.Aliases.MultiNodeTreePicker))
                {
                    var udis = (Udi[])source;
                    var isSingleNodePicker = IsSingleNodePicker(propertyType);

                    if ((propertyType.Alias != null && PropertiesToExclude.InvariantContains(propertyType.Alias)) == false)
                    {
                        var multiNodeTreePicker = new List<IPublishedContent>();

                        var objectType = UmbracoObjectTypes.Unknown;

                        foreach (var udi in udis)
                        {
                            var guidUdi = udi as GuidUdi;
                            if (guidUdi == null) continue;

                            IPublishedContent multiNodeTreePickerItem = null;
                            switch (udi.EntityType)
                            {
                                case Constants.UdiEntityType.Document:
                                    multiNodeTreePickerItem = GetPublishedContent(udi, ref objectType, UmbracoObjectTypes.Document, id => _publishedSnapshotAccessor.PublishedSnapshot.Content.GetById(guidUdi.Guid));
                                    break;
                                case Constants.UdiEntityType.Media:
                                    multiNodeTreePickerItem = GetPublishedContent(udi, ref objectType, UmbracoObjectTypes.Media, id => _publishedSnapshotAccessor.PublishedSnapshot.Media.GetById(guidUdi.Guid));
                                    break;
                                case Constants.UdiEntityType.Member:
                                    multiNodeTreePickerItem = GetPublishedContent(udi, ref objectType, UmbracoObjectTypes.Member, id => _publishedSnapshotAccessor.PublishedSnapshot.Members.GetByProviderKey(guidUdi.Guid));
                                    break;
                            }

                            if (multiNodeTreePickerItem != null && multiNodeTreePickerItem.ContentType.ItemType != PublishedItemType.Element)
                            {
                                multiNodeTreePicker.Add(multiNodeTreePickerItem);
                                if (isSingleNodePicker)
                                {
                                    break;
                                }
                            }
                        }

                        if (isSingleNodePicker)
                        {
                            return multiNodeTreePicker.FirstOrDefault();
                        }
                        return multiNodeTreePicker;
                    }

                    // return the first nodeId as this is one of the excluded properties that expects a single id
                    return udis.FirstOrDefault();
                }
            }
            return source;
        }

        /// <summary>
        /// Attempt to get an IPublishedContent instance based on ID and content type
        /// </summary>
        /// <param name="nodeId">The content node ID</param>
        /// <param name="actualType">The type of content being requested</param>
        /// <param name="expectedType">The type of content expected/supported by <paramref name="contentFetcher"/></param>
        /// <param name="contentFetcher">A function to fetch content of type <paramref name="expectedType"/></param>
        /// <returns>The requested content, or null if either it does not exist or <paramref name="actualType"/> does not match <paramref name="expectedType"/></returns>
        private IPublishedContent GetPublishedContent<T>(T nodeId, ref UmbracoObjectTypes actualType, UmbracoObjectTypes expectedType, Func<T, IPublishedContent> contentFetcher)
        {
            // is the actual type supported by the content fetcher?
            if (actualType != UmbracoObjectTypes.Unknown && actualType != expectedType)
            {
                // no, return null
                return null;
            }

            // attempt to get the content
            var content = contentFetcher(nodeId);
            if (content != null)
            {
                // if we found the content, assign the expected type to the actual type so we don't have to keep looking for other types of content
                actualType = expectedType;
            }
            return content;
        }

        private static bool IsSingleNodePicker(IPublishedPropertyType propertyType)
        {
            return propertyType.DataType.ConfigurationAs<MultiNodePickerConfiguration>().MaxNumber == 1;
        }
    }
}
