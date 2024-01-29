using Examine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Web.Common.DependencyInjection;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Examine;

/// <summary>
///     Builds <see cref="ValueSet" />s for <see cref="IContent" /> items
/// </summary>
public class ContentValueSetBuilder : BaseValueSetBuilder<IContent>, IContentValueSetBuilder,
    IPublishedContentValueSetBuilder
{
    private static readonly object[] NoValue = new[] { "n" };
    private static readonly object[] YesValue = new[] { "y" };

    private readonly ICoreScopeProvider _scopeProvider;

    private readonly IShortStringHelper _shortStringHelper;
    private readonly UrlSegmentProviderCollection _urlSegmentProviders;
    private readonly IUserService _userService;
    private readonly ILocalizationService _localizationService;
    private readonly IContentTypeService _contentTypeService;
    private readonly ILogger<ContentValueSetBuilder> _logger;

    public ContentValueSetBuilder(
        PropertyEditorCollection propertyEditors,
        UrlSegmentProviderCollection urlSegmentProviders,
        IUserService userService,
        IShortStringHelper shortStringHelper,
        ICoreScopeProvider scopeProvider,
        bool publishedValuesOnly,
        ILocalizationService localizationService,
        IContentTypeService contentTypeService,
        ILogger<ContentValueSetBuilder> logger)
        : base(propertyEditors, publishedValuesOnly)
    {
        _urlSegmentProviders = urlSegmentProviders;
        _userService = userService;
        _shortStringHelper = shortStringHelper;
        _scopeProvider = scopeProvider;
        _localizationService = localizationService;
        _contentTypeService = contentTypeService;
        _logger = logger;
    }

    [Obsolete("Use non-obsolete ctor, scheduled for removal in v14")]
    public ContentValueSetBuilder(
        PropertyEditorCollection propertyEditors,
        UrlSegmentProviderCollection urlSegmentProviders,
        IUserService userService,
        IShortStringHelper shortStringHelper,
        ICoreScopeProvider scopeProvider,
        bool publishedValuesOnly,
        ILocalizationService localizationService,
        IContentTypeService contentTypeService)
        : this(
            propertyEditors,
            urlSegmentProviders,
            userService,
            shortStringHelper,
            scopeProvider,
            publishedValuesOnly,
            localizationService,
            StaticServiceProvider.Instance.GetRequiredService<IContentTypeService>(),
            StaticServiceProvider.Instance.GetRequiredService<ILogger<ContentValueSetBuilder>>())
    {
    }

    [Obsolete("Use non-obsolete ctor, scheduled for removal in v14")]
    public ContentValueSetBuilder(
        PropertyEditorCollection propertyEditors,
        UrlSegmentProviderCollection urlSegmentProviders,
        IUserService userService,
        IShortStringHelper shortStringHelper,
        IScopeProvider scopeProvider,
        bool publishedValuesOnly,
        ILocalizationService localizationService)
        : this(
            propertyEditors,
            urlSegmentProviders,
            userService,
            shortStringHelper,
            scopeProvider,
            publishedValuesOnly,
            localizationService,
            StaticServiceProvider.Instance.GetRequiredService<IContentTypeService>(),
            StaticServiceProvider.Instance.GetRequiredService<ILogger<ContentValueSetBuilder>>())
    {
    }

    [Obsolete("Use non-obsolete ctor, scheduled for removal in v14")]
    public ContentValueSetBuilder(
        PropertyEditorCollection propertyEditors,
        UrlSegmentProviderCollection urlSegmentProviders,
        IUserService userService,
        IShortStringHelper shortStringHelper,
        IScopeProvider scopeProvider,
        bool publishedValuesOnly)
        : this(
            propertyEditors,
            urlSegmentProviders,
            userService,
            shortStringHelper,
            scopeProvider,
            publishedValuesOnly,
            StaticServiceProvider.Instance.GetRequiredService<ILocalizationService>(),
            StaticServiceProvider.Instance.GetRequiredService<IContentTypeService>(),
            StaticServiceProvider.Instance.GetRequiredService<ILogger<ContentValueSetBuilder>>())
    {
    }

    /// <inheritdoc />
    public override IEnumerable<ValueSet> GetValueSets(params IContent[] content)
    {
        Dictionary<int, IProfile> creatorIds;
        Dictionary<int, IProfile> writerIds;

        // We can lookup all of the creator/writer names at once which can save some
        // processing below instead of one by one.
        using (ICoreScope scope = _scopeProvider.CreateCoreScope())
        {
            creatorIds = _userService.GetProfilesById(content.Select(x => x.CreatorId).ToArray())
                .ToDictionary(x => x.Id, x => x);
            writerIds = _userService.GetProfilesById(content.Select(x => x.WriterId).ToArray())
                .ToDictionary(x => x.Id, x => x);
            scope.Complete();
        }

        return GetValueSetsEnumerable(content, creatorIds, writerIds);
    }

    private IEnumerable<ValueSet> GetValueSetsEnumerable(IContent[] content, Dictionary<int, IProfile> creatorIds, Dictionary<int, IProfile> writerIds)
    {
        IDictionary<Guid, IContentType> contentTypeDictionary = _contentTypeService.GetAll().ToDictionary(x => x.Key);

        // TODO: There is a lot of boxing going on here and ultimately all values will be boxed by Lucene anyways
        // but I wonder if there's a way to reduce the boxing that we have to do or if it will matter in the end since
        // Lucene will do it no matter what? One idea was to create a `FieldValue` struct which would contain `object`, `object[]`, `ValueType` and `ValueType[]`
        // references and then each array is an array of `FieldValue[]` and values are assigned accordingly. Not sure if it will make a difference or not.
        foreach (IContent c in content)
        {
            var isVariant = c.ContentType.VariesByCulture();

            var urlValue = c.GetUrlSegment(_shortStringHelper, _urlSegmentProviders); // Always add invariant urlName
            var values = new Dictionary<string, IEnumerable<object?>>
            {
                { "icon", c.ContentType.Icon?.Yield() ?? Enumerable.Empty<string>() },
                {
                    UmbracoExamineFieldNames.PublishedFieldName, c.Published ? YesValue : NoValue
                }, // Always add invariant published value
                { "id", new object[] { c.Id } },
                { UmbracoExamineFieldNames.NodeKeyFieldName, new object[] { c.Key } },
                { "parentID", new object[] { c.Level > 1 ? c.ParentId : -1 } },
                { "level", new object[] { c.Level } },
                { "creatorID", new object[] { c.CreatorId } },
                { "sortOrder", new object[] { c.SortOrder } },
                { "createDate", new object[] { c.CreateDate } }, // Always add invariant createDate
                { "updateDate", new object[] { c.UpdateDate } }, // Always add invariant updateDate
                {
                    UmbracoExamineFieldNames.NodeNameFieldName, (PublishedValuesOnly // Always add invariant nodeName
                        ? c.PublishName?.Yield()
                        : c.Name?.Yield()) ?? Enumerable.Empty<string>()
                },
                { "urlName", urlValue?.Yield() ?? Enumerable.Empty<string>() }, // Always add invariant urlName
                { "path", c.Path.Yield() },
                { "nodeType", c.ContentType.Id.ToString().Yield() },
                {
                    "creatorName",
                    (creatorIds.TryGetValue(c.CreatorId, out IProfile? creatorProfile) ? creatorProfile.Name! : "??")
                    .Yield()
                },
                {
                    "writerName",
                    (writerIds.TryGetValue(c.WriterId, out IProfile? writerProfile) ? writerProfile.Name! : "??")
                    .Yield()
                },
                { "writerID", new object[] { c.WriterId } },
                { "templateID", new object[] { c.TemplateId ?? 0 } },
                { UmbracoExamineFieldNames.VariesByCultureFieldName, NoValue },
            };

            if (isVariant)
            {
                values[UmbracoExamineFieldNames.VariesByCultureFieldName] = YesValue;

                foreach (var culture in c.AvailableCultures)
                {
                    var variantUrl = c.GetUrlSegment(_shortStringHelper, _urlSegmentProviders, culture);
                    var lowerCulture = culture.ToLowerInvariant();
                    values[$"urlName_{lowerCulture}"] = variantUrl?.Yield() ?? Enumerable.Empty<string>();
                    values[$"nodeName_{lowerCulture}"] = (PublishedValuesOnly
                        ? c.GetPublishName(culture)?.Yield()
                        : c.GetCultureName(culture)?.Yield()) ?? Enumerable.Empty<string>();
                    values[$"{UmbracoExamineFieldNames.PublishedFieldName}_{lowerCulture}"] =
                        (c.IsCulturePublished(culture) ? "y" : "n").Yield<object>();
                    values[$"updateDate_{lowerCulture}"] = (PublishedValuesOnly
                        ? c.GetPublishDate(culture)
                        : c.GetUpdateDate(culture))?.Yield<object>() ?? Enumerable.Empty<object>();
                }
            }

            var availableCultures = new List<string>(c.AvailableCultures);
            if (availableCultures.Any() is false)
            {
                availableCultures.Add(_localizationService.GetDefaultLanguageIsoCode());
            }

            foreach (IProperty property in c.Properties)
            {
                if (!property.PropertyType.VariesByCulture())
                {
                    try
                    {
                        AddPropertyValue(property, null, null, values, availableCultures, contentTypeDictionary);
                    }
                    catch (JsonSerializationException ex)
                    {
                        _logger.LogError(ex, "Failed to add property '{PropertyAlias}' to index for content {ContentId}", property.Alias, c.Id);
                        throw;
                    }
                }
                else
                {
                    foreach (var culture in c.AvailableCultures)
                    {
                        try
                        {
                            AddPropertyValue(property, culture.ToLowerInvariant(), null, values, availableCultures, contentTypeDictionary);
                        }
                        catch (JsonSerializationException ex)
                        {
                            _logger.LogError(
                                ex,
                                "Failed to add property '{PropertyAlias}' to index for content {ContentId} in culture {Culture}",
                                property.Alias,
                                c.Id,
                                culture);
                            throw;
                        }
                    }
                }
            }

            var vs = new ValueSet(c.Id.ToInvariantString(), IndexTypes.Content, c.ContentType.Alias, values);

            yield return vs;
        }
    }
}
