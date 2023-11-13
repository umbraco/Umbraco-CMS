using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors.DeliveryApi;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.DependencyInjection;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

/// <summary>
///     The multi node tree picker property editor value converter.
/// </summary>
[DefaultPropertyValueConverter(typeof(MustBeStringValueConverter))]
public class MultiNodeTreePickerValueConverter : PropertyValueConverterBase, IDeliveryApiPropertyValueConverter
{
    private static readonly List<string> PropertiesToExclude = new()
    {
        Constants.Conventions.Content.InternalRedirectId.ToLower(CultureInfo.InvariantCulture),
        Constants.Conventions.Content.Redirect.ToLower(CultureInfo.InvariantCulture),
    };

    private readonly IMemberService _memberService;
    private readonly IPublishedSnapshotAccessor _publishedSnapshotAccessor;
    private readonly IUmbracoContextAccessor _umbracoContextAccessor;
    private readonly IApiContentBuilder _apiContentBuilder;
    private readonly IApiMediaBuilder _apiMediaBuilder;

    [Obsolete("Use constructor that takes all parameters, scheduled for removal in V14")]
    public MultiNodeTreePickerValueConverter(
        IPublishedSnapshotAccessor publishedSnapshotAccessor,
        IUmbracoContextAccessor umbracoContextAccessor,
        IMemberService memberService)
        : this(
            publishedSnapshotAccessor,
            umbracoContextAccessor,
            memberService,
            StaticServiceProvider.Instance.GetRequiredService<IApiContentBuilder>(),
            StaticServiceProvider.Instance.GetRequiredService<IApiMediaBuilder>())
    {
    }

    public MultiNodeTreePickerValueConverter(
        IPublishedSnapshotAccessor publishedSnapshotAccessor,
        IUmbracoContextAccessor umbracoContextAccessor,
        IMemberService memberService,
        IApiContentBuilder apiContentBuilder,
        IApiMediaBuilder apiMediaBuilder)
    {
        _publishedSnapshotAccessor = publishedSnapshotAccessor ??
                                     throw new ArgumentNullException(nameof(publishedSnapshotAccessor));
        _umbracoContextAccessor = umbracoContextAccessor;
        _memberService = memberService;
        _apiContentBuilder = apiContentBuilder;
        _apiMediaBuilder = apiMediaBuilder;
    }

    public override bool IsConverter(IPublishedPropertyType propertyType) =>
        propertyType.EditorAlias.Equals(Constants.PropertyEditors.Aliases.MultiNodeTreePicker);

    public override PropertyCacheLevel GetPropertyCacheLevel(IPublishedPropertyType propertyType)
        => PropertyCacheLevel.Snapshot;

    public override Type GetPropertyValueType(IPublishedPropertyType propertyType)
        => IsSingleNodePicker(propertyType)
            ? typeof(IPublishedContent)
            : typeof(IEnumerable<IPublishedContent>);

    public override object? ConvertSourceToIntermediate(IPublishedElement owner, IPublishedPropertyType propertyType, object? source, bool preview)
    {
        if (source == null)
        {
            return null;
        }

        if (propertyType.EditorAlias.Equals(Constants.PropertyEditors.Aliases.MultiNodeTreePicker))
        {
            Udi[]? nodeIds = source.ToString()?
                .Split(Constants.CharArrays.Comma, StringSplitOptions.RemoveEmptyEntries)
                .Select(UdiParser.Parse)
                .ToArray();
            return nodeIds;
        }

        return null;
    }

    public override object? ConvertIntermediateToObject(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel cacheLevel, object? source, bool preview)
    {
        if (source == null)
        {
            return null;
        }

        // TODO: Inject an UmbracoHelper and create a GetUmbracoHelper method based on either injected or singleton
        if (_umbracoContextAccessor.TryGetUmbracoContext(out _))
        {
            if (propertyType.EditorAlias.Equals(Constants.PropertyEditors.Aliases.MultiNodeTreePicker))
            {
                var udis = (Udi[])source;
                var isSingleNodePicker = IsSingleNodePicker(propertyType);

                if ((propertyType.Alias != null && PropertiesToExclude.InvariantContains(propertyType.Alias)) == false)
                {
                    var multiNodeTreePicker = new List<IPublishedContent>();

                    UmbracoObjectTypes objectType = UmbracoObjectTypes.Unknown;
                    IPublishedSnapshot publishedSnapshot = _publishedSnapshotAccessor.GetRequiredPublishedSnapshot();
                    foreach (Udi udi in udis)
                    {
                        if (udi is not GuidUdi guidUdi)
                        {
                            continue;
                        }

                        IPublishedContent? multiNodeTreePickerItem = null;
                        switch (udi.EntityType)
                        {
                            case Constants.UdiEntityType.Document:
                                multiNodeTreePickerItem = GetPublishedContent(
                                    udi,
                                    ref objectType,
                                    UmbracoObjectTypes.Document,
                                    id => publishedSnapshot.Content?.GetById(guidUdi.Guid));
                                break;
                            case Constants.UdiEntityType.Media:
                                multiNodeTreePickerItem = GetPublishedContent(
                                    udi,
                                    ref objectType,
                                    UmbracoObjectTypes.Media,
                                    id => publishedSnapshot.Media?.GetById(guidUdi.Guid));
                                break;
                            case Constants.UdiEntityType.Member:
                                multiNodeTreePickerItem = GetPublishedContent(
                                    udi,
                                    ref objectType,
                                    UmbracoObjectTypes.Member,
                                    id =>
                                    {
                                        IMember? m = _memberService.GetByKey(guidUdi.Guid);
                                        if (m == null)
                                        {
                                            return null;
                                        }

                                        IPublishedContent? member = publishedSnapshot?.Members?.Get(m);
                                        return member;
                                    });
                                break;
                        }

                        if (multiNodeTreePickerItem != null &&
                            multiNodeTreePickerItem.ContentType.ItemType != PublishedItemType.Element)
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

    public PropertyCacheLevel GetDeliveryApiPropertyCacheLevel(IPublishedPropertyType propertyType) => PropertyCacheLevel.Elements;

    public PropertyCacheLevel GetDeliveryApiPropertyCacheLevelForExpansion(IPublishedPropertyType propertyType) => PropertyCacheLevel.Snapshot;

    public Type GetDeliveryApiPropertyValueType(IPublishedPropertyType propertyType)
        => GetEntityType(propertyType) switch
        {
            Constants.UdiEntityType.Media => typeof(IEnumerable<IApiMedia>),
            Constants.UdiEntityType.Member => typeof(string), // unsupported
            _ => typeof(IEnumerable<IApiContent>)
        };


    public object? ConvertIntermediateToDeliveryApiObject(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object? inter, bool preview, bool expanding)
    {
        IEnumerable<IApiContent> DefaultValue() => Array.Empty<IApiContent>();

        if (inter is not IEnumerable<Udi> udis)
        {
            return DefaultValue();
        }

        IPublishedSnapshot publishedSnapshot = _publishedSnapshotAccessor.GetRequiredPublishedSnapshot();

        var entityType = GetEntityType(propertyType);

        if (entityType == "content")
        {
            // TODO Why do MNTP config saves "content" and not "document"?
            entityType = Constants.UdiEntityType.Document;
        }

        GuidUdi[] entityTypeUdis = udis.Where(udi => udi.EntityType == entityType).OfType<GuidUdi>().ToArray();
        return entityType switch
        {
            Constants.UdiEntityType.Document => entityTypeUdis.Select(udi =>
            {
                IPublishedContent? content = publishedSnapshot.Content?.GetById(udi.Guid);
                return content != null
                    ? _apiContentBuilder.Build(content)
                    : null;
            }).WhereNotNull().ToArray(),
            Constants.UdiEntityType.Media => entityTypeUdis.Select(udi =>
            {
                IPublishedContent? media = publishedSnapshot.Media?.GetById(udi.Guid);
                return media != null
                    ? _apiMediaBuilder.Build(media)
                    : null;
            }).WhereNotNull().ToArray(),
            Constants.UdiEntityType.Member => "(unsupported)",
            _ => DefaultValue()
        };
    }

    private static bool IsSingleNodePicker(IPublishedPropertyType propertyType) =>
        propertyType.DataType.ConfigurationAs<MultiNodePickerConfiguration>()?.MaxNumber == 1;

    private static string GetEntityType(IPublishedPropertyType propertyType) =>
        propertyType.DataType.ConfigurationAs<MultiNodePickerConfiguration>()?.TreeSource?.ObjectType ?? Constants.UdiEntityType.Document;

    /// <summary>
    ///     Attempt to get an IPublishedContent instance based on ID and content type
    /// </summary>
    /// <param name="nodeId">The content node ID</param>
    /// <param name="actualType">The type of content being requested</param>
    /// <param name="expectedType">The type of content expected/supported by <paramref name="contentFetcher" /></param>
    /// <param name="contentFetcher">A function to fetch content of type <paramref name="expectedType" /></param>
    /// <returns>
    ///     The requested content, or null if either it does not exist or <paramref name="actualType" /> does not match
    ///     <paramref name="expectedType" />
    /// </returns>
    private IPublishedContent? GetPublishedContent<T>(T nodeId, ref UmbracoObjectTypes actualType, UmbracoObjectTypes expectedType, Func<T, IPublishedContent?> contentFetcher)
    {
        // is the actual type supported by the content fetcher?
        if (actualType != UmbracoObjectTypes.Unknown && actualType != expectedType)
        {
            // no, return null
            return null;
        }

        // attempt to get the content
        IPublishedContent? content = contentFetcher(nodeId);
        if (content != null)
        {
            // if we found the content, assign the expected type to the actual type so we don't have to keep looking for other types of content
            actualType = expectedType;
        }

        return content;
    }
}
