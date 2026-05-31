using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core;

/// <summary>
///     Implements <see cref="ITagQuery" />.
/// </summary>
public class TagQuery : ITagQuery
{
    private readonly IPublishedContentQuery _contentQuery;
    private readonly IUmbracoMapper _mapper;
    private readonly ITagService _tagService;

    /// <summary>
    /// Initializes a new instance of the <see cref="TagQuery" /> class.
    /// </summary>
    /// <param name="tagService">The service used to manage and query tags.</param>
    /// <param name="contentQuery">The service used to query published content.</param>
    /// <param name="mapper">The mapper used for mapping Umbraco objects.</param>
    public TagQuery(ITagService tagService, IPublishedContentQuery contentQuery, IUmbracoMapper mapper)
    {
        _tagService = tagService ?? throw new ArgumentNullException(nameof(tagService));
        _contentQuery = contentQuery ?? throw new ArgumentNullException(nameof(contentQuery));
        _mapper = mapper;
    }

    /// <inheritdoc />
    public IEnumerable<IPublishedContent> GetContentByTag(string tag, string? group = null, string? culture = null)
    {
        IEnumerable<int> ids = _tagService.GetTaggedContentByTag(tag, group, culture)
            .Select(x => x.EntityId);
        return _contentQuery.Content(ids);
    }

    /// <inheritdoc />
    public IEnumerable<IPublishedContent> GetContentByTagGroup(string group, string? culture = null)
    {
        IEnumerable<int> ids = _tagService.GetTaggedContentByTagGroup(group, culture)
            .Select(x => x.EntityId);
        return _contentQuery.Content(ids);
    }

    /// <inheritdoc />
    public IEnumerable<IPublishedContent> GetMediaByTag(string tag, string? group = null, string? culture = null)
    {
        IEnumerable<int> ids = _tagService.GetTaggedMediaByTag(tag, group, culture)
            .Select(x => x.EntityId);
        return _contentQuery.Media(ids);
    }

    /// <inheritdoc />
    public IEnumerable<IPublishedContent> GetMediaByTagGroup(string group, string? culture = null)
    {
        IEnumerable<int> ids = _tagService.GetTaggedMediaByTagGroup(group, culture)
            .Select(x => x.EntityId);
        return _contentQuery.Media(ids);
    }

    /// <inheritdoc />
    public IEnumerable<TagModel?> GetAllTags(string? group = null, string? culture = null) =>
        _mapper.MapEnumerable<ITag, TagModel>(_tagService.GetAllTags(group, culture));

    /// <inheritdoc />
    public IEnumerable<TagModel?> GetAllContentTags(string? group = null, string? culture = null) =>
        _mapper.MapEnumerable<ITag, TagModel>(_tagService.GetAllContentTags(group, culture));

    /// <inheritdoc />
    public IEnumerable<TagModel?> GetAllMediaTags(string? group = null, string? culture = null) =>
        _mapper.MapEnumerable<ITag, TagModel>(_tagService.GetAllMediaTags(group, culture));

    /// <inheritdoc />
    public IEnumerable<TagModel?> GetAllMemberTags(string? group = null, string? culture = null) =>
        _mapper.MapEnumerable<ITag, TagModel>(_tagService.GetAllMemberTags(group, culture));

    /// <inheritdoc />
    public IEnumerable<TagModel?> GetTagsForProperty(int contentId, string propertyTypeAlias, string? group = null, string? culture = null) =>
        _mapper.MapEnumerable<ITag, TagModel>(_tagService.GetTagsForProperty(contentId, propertyTypeAlias, group, culture));

    /// <inheritdoc />
    public IEnumerable<TagModel?> GetTagsForEntity(int contentId, string? group = null, string? culture = null) =>
        _mapper.MapEnumerable<ITag, TagModel>(_tagService.GetTagsForEntity(contentId, group, culture));
}
