using Examine;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Examine;

public class MediaValueSetBuilder : BaseValueSetBuilder<IMedia>
{
    private readonly ContentSettings _contentSettings;
    private readonly MediaUrlGeneratorCollection _mediaUrlGenerators;
    private readonly IShortStringHelper _shortStringHelper;
    private readonly UrlSegmentProviderCollection _urlSegmentProviders;
    private readonly IUserService _userService;

    public MediaValueSetBuilder(
        PropertyEditorCollection propertyEditors,
        UrlSegmentProviderCollection urlSegmentProviders,
        MediaUrlGeneratorCollection mediaUrlGenerators,
        IUserService userService,
        IShortStringHelper shortStringHelper,
        IOptions<ContentSettings> contentSettings)
        : base(propertyEditors, false)
    {
        _urlSegmentProviders = urlSegmentProviders;
        _mediaUrlGenerators = mediaUrlGenerators;
        _userService = userService;
        _shortStringHelper = shortStringHelper;
        _contentSettings = contentSettings.Value;
    }

    /// <inheritdoc />
    public override IEnumerable<ValueSet> GetValueSets(params IMedia[] media)
    {
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
                AddPropertyValue(property, null, null, values);
            }

            var vs = new ValueSet(m.Id.ToInvariantString(), IndexTypes.Media, m.ContentType.Alias, values);

            yield return vs;
        }
    }
}
