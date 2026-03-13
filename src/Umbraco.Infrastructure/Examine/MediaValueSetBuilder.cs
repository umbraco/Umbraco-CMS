using Examine;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Examine;

/// <summary>
/// Responsible for constructing <see cref="ValueSet"/> instances representing media items
/// for indexing by the Examine search engine in Umbraco.
/// This builder extracts relevant properties from media entities to facilitate efficient searching and retrieval.
/// </summary>
public class MediaValueSetBuilder : BaseValueSetBuilder<IMedia>
{
    private readonly ContentSettings _contentSettings;
    private readonly MediaUrlGeneratorCollection _mediaUrlGenerators;
    private readonly IShortStringHelper _shortStringHelper;
    private readonly IContentTypeService _contentTypeService;
    private readonly UrlSegmentProviderCollection _urlSegmentProviders;
    private readonly IUserService _userService;

    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Infrastructure.Examine.MediaValueSetBuilder"/> class.
    /// </summary>
    /// <param name="propertyEditors">A collection of <see cref="PropertyEditorCollection"/> used to manage property editors for media items.</param>
    /// <param name="urlSegmentProviders">A collection of <see cref="UrlSegmentProviderCollection"/> used to generate URL segments for media items.</param>
    /// <param name="mediaUrlGenerators">A collection of <see cref="MediaUrlGeneratorCollection"/> used to generate URLs for media items.</param>
    /// <param name="userService">An implementation of <see cref="IUserService"/> for accessing user-related operations.</param>
    /// <param name="shortStringHelper">An implementation of <see cref="IShortStringHelper"/> for string manipulation and formatting.</param>
    /// <param name="contentSettings">The <see cref="IOptions{ContentSettings}"/> providing configuration options for content.</param>
    /// <param name="contentTypeService">An implementation of <see cref="IContentTypeService"/> for accessing content type information.</param>
    public MediaValueSetBuilder(
        PropertyEditorCollection propertyEditors,
        UrlSegmentProviderCollection urlSegmentProviders,
        MediaUrlGeneratorCollection mediaUrlGenerators,
        IUserService userService,
        IShortStringHelper shortStringHelper,
        IOptions<ContentSettings> contentSettings,
        IContentTypeService contentTypeService)
        : base(propertyEditors, false)
    {
        _urlSegmentProviders = urlSegmentProviders;
        _mediaUrlGenerators = mediaUrlGenerators;
        _userService = userService;
        _shortStringHelper = shortStringHelper;
        _contentTypeService = contentTypeService;
        _contentSettings = contentSettings.Value;
    }

    /// <inheritdoc />
    public override IEnumerable<ValueSet> GetValueSets(params IMedia[] media)
    {
        IDictionary<Guid, IContentType> contentTypeDictionary = _contentTypeService.GetAll().ToDictionary(x => x.Key);

        foreach (IMedia m in media)
        {
            var urlValue = m.GetUrlSegment(_shortStringHelper, _urlSegmentProviders);

            IEnumerable<string?> mediaFiles = m.GetUrls(_contentSettings, _mediaUrlGenerators)
                .Select(x => Path.GetFileName(x))
                .Distinct();

            var values = new Dictionary<string, IEnumerable<object?>>
            {
                { "icon", m.ContentType.Icon?.Yield() ?? Enumerable.Empty<string>() },
                { "id", new object[] { m.Id } },
                { UmbracoExamineFieldNames.NodeKeyFieldName, new object[] { m.Key } },
                { "parentID", new object[] { m.Level > 1 ? m.ParentId : -1 } },
                { "level", new object[] { m.Level } },
                { "creatorID", new object[] { m.CreatorId } },
                { "sortOrder", new object[] { m.SortOrder } },
                { "createDate", new object[] { m.CreateDate } },
                { "updateDate", new object[] { m.UpdateDate } },
                { UmbracoExamineFieldNames.NodeNameFieldName, m.Name?.Yield() ?? Enumerable.Empty<string>() },
                { "urlName", urlValue?.Yield() ?? Enumerable.Empty<string>() },
                { "path", m.Path.Yield() },
                { "nodeType", m.ContentType.Id.ToString().Yield() },
                { "creatorName", (m.GetCreatorProfile(_userService)?.Name ?? "??").Yield() },
                { UmbracoExamineFieldNames.UmbracoFileFieldName, mediaFiles },
            };

            foreach (IProperty property in m.Properties)
            {
                AddPropertyValue(property, null, null, values, m.AvailableCultures, contentTypeDictionary);
            }

            var vs = new ValueSet(m.Id.ToInvariantString(), IndexTypes.Media, m.ContentType.Alias, values);

            yield return vs;
        }
    }
}
