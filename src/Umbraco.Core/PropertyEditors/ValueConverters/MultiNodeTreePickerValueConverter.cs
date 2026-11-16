using System.Globalization;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors.DeliveryApi;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
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
    private readonly IUmbracoContextAccessor _umbracoContextAccessor;
    private readonly IApiContentBuilder _apiContentBuilder;
    private readonly IApiMediaBuilder _apiMediaBuilder;
    private readonly IPublishedContentCache _contentCache;
    private readonly IPublishedMediaCache _mediaCache;
    private readonly IPublishedMemberCache _memberCache;

    /// <summary>
    ///     Initializes a new instance of the <see cref="MultiNodeTreePickerValueConverter" /> class.
    /// </summary>
    /// <param name="umbracoContextAccessor">The Umbraco context accessor.</param>
    /// <param name="memberService">The member service.</param>
    /// <param name="apiContentBuilder">The API content builder.</param>
    /// <param name="apiMediaBuilder">The API media builder.</param>
    /// <param name="contentCache">The published content cache.</param>
    /// <param name="mediaCache">The published media cache.</param>
    /// <param name="memberCache">The published member cache.</param>
    public MultiNodeTreePickerValueConverter(
        IUmbracoContextAccessor umbracoContextAccessor,
        IMemberService memberService,
        IApiContentBuilder apiContentBuilder,
        IApiMediaBuilder apiMediaBuilder,
        IPublishedContentCache contentCache,
        IPublishedMediaCache mediaCache,
        IPublishedMemberCache memberCache)
    {
        _umbracoContextAccessor = umbracoContextAccessor;
        _memberService = memberService;
        _apiContentBuilder = apiContentBuilder;
        _apiMediaBuilder = apiMediaBuilder;
        _contentCache = contentCache;
        _mediaCache = mediaCache;
        _memberCache = memberCache;
    }

    /// <inheritdoc />
    public override bool IsConverter(IPublishedPropertyType propertyType) =>
        propertyType.EditorAlias.Equals(Constants.PropertyEditors.Aliases.MultiNodeTreePicker);

    /// <inheritdoc />
    public override PropertyCacheLevel GetPropertyCacheLevel(IPublishedPropertyType propertyType)
        => PropertyCacheLevel.Snapshot;

    /// <inheritdoc />
    public override Type GetPropertyValueType(IPublishedPropertyType propertyType)
        => IsSingleNodePicker(propertyType)
            ? typeof(IPublishedContent)
            : typeof(IEnumerable<IPublishedContent>);

    /// <inheritdoc />
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

    /// <inheritdoc />
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

                    // The documents and media of the value are each read in one batch, the first time one of them is
                    // looked up, rather than once per item.
                    Dictionary<Guid, IPublishedContent>? documents = null;
                    Dictionary<Guid, IPublishedContent>? media = null;

                    UmbracoObjectTypes objectType = UmbracoObjectTypes.Unknown;
                    foreach (Udi udi in udis)
                    {
                        if (udi is not GuidUdi guidUdi)
                        {
                            continue;
                        }

                        UmbracoObjectTypes entityObjectType = udi.EntityType switch
                        {
                            Constants.UdiEntityType.Document => UmbracoObjectTypes.Document,
                            Constants.UdiEntityType.Media => UmbracoObjectTypes.Media,
                            Constants.UdiEntityType.Member => UmbracoObjectTypes.Member,
                            _ => UmbracoObjectTypes.Unknown,
                        };

                        // Once an item has been found, only items of its entity type are considered.
                        if (entityObjectType == UmbracoObjectTypes.Unknown
                            || (objectType != UmbracoObjectTypes.Unknown && objectType != entityObjectType))
                        {
                            continue;
                        }

                        IPublishedContent? multiNodeTreePickerItem = entityObjectType switch
                        {
                            UmbracoObjectTypes.Document => (documents ??= ReadBatch(udis, Constants.UdiEntityType.Document, keys => _contentCache.GetByIds(preview, keys)))
                                .GetValueOrDefault(guidUdi.Guid),
                            UmbracoObjectTypes.Media => (media ??= ReadBatch(udis, Constants.UdiEntityType.Media, _mediaCache.GetByIds))
                                .GetValueOrDefault(guidUdi.Guid),
                            _ => GetMember(guidUdi.Guid),
                        };

                        if (multiNodeTreePickerItem is null)
                        {
                            continue;
                        }

                        objectType = entityObjectType;

                        if (multiNodeTreePickerItem.ContentType.ItemType == PublishedItemType.Element)
                        {
                            continue;
                        }

                        multiNodeTreePicker.Add(multiNodeTreePickerItem);
                        if (isSingleNodePicker)
                        {
                            break;
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

    /// <inheritdoc />
    public PropertyCacheLevel GetDeliveryApiPropertyCacheLevel(IPublishedPropertyType propertyType) => PropertyCacheLevel.Snapshot;

    /// <inheritdoc />
    public PropertyCacheLevel GetDeliveryApiPropertyCacheLevelForExpansion(IPublishedPropertyType propertyType) => PropertyCacheLevel.Snapshot;

    /// <inheritdoc />
    public Type GetDeliveryApiPropertyValueType(IPublishedPropertyType propertyType)
        => GetEntityType(propertyType) switch
        {
            Constants.UdiEntityType.Media => typeof(IEnumerable<IApiMedia>),
            Constants.UdiEntityType.Member => typeof(string), // unsupported
            _ => typeof(IEnumerable<IApiContent>)
        };


    /// <inheritdoc />
    public object? ConvertIntermediateToDeliveryApiObject(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object? inter, bool preview, bool expanding)
    {
        IEnumerable<IApiContent> DefaultValue() => Array.Empty<IApiContent>();

        if (inter is not IEnumerable<Udi> udis)
        {
            return DefaultValue();
        }

        var entityType = GetEntityType(propertyType);

        if (entityType == "content")
        {
            // TODO Why do MNTP config saves "content" and not "document"?
            entityType = Constants.UdiEntityType.Document;
        }

        GuidUdi[] entityTypeUdis = udis.Where(udi => udi.EntityType == entityType).OfType<GuidUdi>().ToArray();
        return entityType switch
        {
            Constants.UdiEntityType.Document => _contentCache
                .GetByIds(preview, Array.ConvertAll(entityTypeUdis, udi => udi.Guid))
                .Select(content => _apiContentBuilder.Build(content))
                .WhereNotNull()
                .ToArray(),
            Constants.UdiEntityType.Media => _mediaCache
                .GetByIds(Array.ConvertAll(entityTypeUdis, udi => udi.Guid))
                .Select(media => _apiMediaBuilder.Build(media))
                .WhereNotNull()
                .ToArray(),
            Constants.UdiEntityType.Member => "(unsupported)",
            _ => DefaultValue()
        };
    }

    private static Dictionary<Guid, IPublishedContent> ReadBatch(
        IEnumerable<Udi> udis,
        string entityType,
        Func<IReadOnlyCollection<Guid>, IReadOnlyList<IPublishedContent>> read)
    {
        Guid[] keys = udis
            .OfType<GuidUdi>()
            .Where(udi => udi.EntityType == entityType)
            .Select(udi => udi.Guid)
            .Distinct()
            .ToArray();

        var items = new Dictionary<Guid, IPublishedContent>(keys.Length);
        foreach (IPublishedContent item in read(keys))
        {
            items[item.Key] = item;
        }

        return items;
    }

    private static bool IsSingleNodePicker(IPublishedPropertyType propertyType) =>
        propertyType.DataType.ConfigurationAs<MultiNodePickerConfiguration>()?.MaxNumber == 1;

    private static string GetEntityType(IPublishedPropertyType propertyType) =>
        propertyType.DataType.ConfigurationAs<MultiNodePickerConfiguration>()?.TreeSource?.ObjectType ?? Constants.UdiEntityType.Document;

    private IPublishedContent? GetMember(Guid key)
    {
        IMember? member = _memberService.GetById(key);
        return member is null ? null : _memberCache.Get(member);
    }
}
