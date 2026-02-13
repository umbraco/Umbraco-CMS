using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Tag service to query for tags in the tags db table. The tags returned are only relevant for published content &amp;
///     saved media or members
/// </summary>
/// <remarks>
///     If there is unpublished content with tags, those tags will not be contained
/// </remarks>
public class TagService : RepositoryService, ITagService
{
    private readonly ITagRepository _tagRepository;

    /// <summary>
    ///     Initializes a new instance of the <see cref="TagService" /> class.
    /// </summary>
    /// <param name="provider">The scope provider for unit of work operations.</param>
    /// <param name="loggerFactory">The logger factory for creating loggers.</param>
    /// <param name="eventMessagesFactory">The factory for creating event messages.</param>
    /// <param name="tagRepository">The repository for tag data access.</param>
    public TagService(ICoreScopeProvider provider, ILoggerFactory loggerFactory, IEventMessagesFactory eventMessagesFactory, ITagRepository tagRepository)
        : base(provider, loggerFactory, eventMessagesFactory) =>
        _tagRepository = tagRepository;

    /// <inheritdoc />
    public TaggedEntity? GetTaggedEntityById(int id)
    {
        using (ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return _tagRepository.GetTaggedEntityById(id);
        }
    }

    /// <inheritdoc />
    public TaggedEntity? GetTaggedEntityByKey(Guid key)
    {
        using (ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return _tagRepository.GetTaggedEntityByKey(key);
        }
    }

    /// <inheritdoc />
    public IEnumerable<TaggedEntity> GetTaggedContentByTagGroup(string group, string? culture = null)
    {
        using (ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return _tagRepository.GetTaggedEntitiesByTagGroup(TaggableObjectTypes.Content, group, culture);
        }
    }

    /// <inheritdoc />
    public IEnumerable<TaggedEntity> GetTaggedContentByTag(string tag, string? group = null, string? culture = null)
    {
        using (ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return _tagRepository.GetTaggedEntitiesByTag(TaggableObjectTypes.Content, tag, group, culture);
        }
    }

    /// <inheritdoc />
    public IEnumerable<TaggedEntity> GetTaggedMediaByTagGroup(string group, string? culture = null)
    {
        using (ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return _tagRepository.GetTaggedEntitiesByTagGroup(TaggableObjectTypes.Media, group, culture);
        }
    }

    /// <inheritdoc />
    public IEnumerable<TaggedEntity> GetTaggedMediaByTag(string tag, string? group = null, string? culture = null)
    {
        using (ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return _tagRepository.GetTaggedEntitiesByTag(TaggableObjectTypes.Media, tag, group, culture);
        }
    }

    /// <inheritdoc />
    public IEnumerable<TaggedEntity> GetTaggedMembersByTagGroup(string group, string? culture = null)
    {
        using (ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return _tagRepository.GetTaggedEntitiesByTagGroup(TaggableObjectTypes.Member, group, culture);
        }
    }

    /// <inheritdoc />
    public IEnumerable<TaggedEntity> GetTaggedMembersByTag(string tag, string? group = null, string? culture = null)
    {
        using (ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return _tagRepository.GetTaggedEntitiesByTag(TaggableObjectTypes.Member, tag, group, culture);
        }
    }

    /// <inheritdoc />
    public IEnumerable<ITag> GetAllTags(string? group = null, string? culture = null)
    {
        using (ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return _tagRepository.GetTagsForEntityType(TaggableObjectTypes.All, group, culture);
        }
    }

    /// <summary>
    ///     Gets all tags asynchronously, optionally filtered by group and culture.
    /// </summary>
    /// <param name="group">The optional tag group to filter by.</param>
    /// <param name="culture">The optional culture to filter by.</param>
    /// <returns>A task that represents the asynchronous operation containing the tags.</returns>
    public Task<IEnumerable<ITag>> GetAllAsync(string? group = null, string? culture = null)
    {
        if (culture == string.Empty)
        {
            culture = null;
        }

        return Task.FromResult(GetAllTags(group, culture));
    }

    /// <summary>
    ///     Gets tags matching the specified query text, optionally filtered by group and culture.
    /// </summary>
    /// <param name="query">The text to search for in tag names.</param>
    /// <param name="group">The optional tag group to filter by.</param>
    /// <param name="culture">The optional culture to filter by.</param>
    /// <returns>A task that represents the asynchronous operation containing the matching tags.</returns>
    public async Task<IEnumerable<ITag>> GetByQueryAsync(string query, string? group = null, string? culture = null) => (await GetAllAsync(group, culture)).Where(x => x.Text.InvariantContains(query));

    /// <inheritdoc />
    public IEnumerable<ITag> GetAllContentTags(string? group = null, string? culture = null)
    {
        using (ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return _tagRepository.GetTagsForEntityType(TaggableObjectTypes.Content, group, culture);
        }
    }

    /// <inheritdoc />
    public IEnumerable<ITag> GetAllMediaTags(string? group = null, string? culture = null)
    {
        using (ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return _tagRepository.GetTagsForEntityType(TaggableObjectTypes.Media, group, culture);
        }
    }

    /// <inheritdoc />
    public IEnumerable<ITag> GetAllMemberTags(string? group = null, string? culture = null)
    {
        using (ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return _tagRepository.GetTagsForEntityType(TaggableObjectTypes.Member, group, culture);
        }
    }

    /// <inheritdoc />
    public IEnumerable<ITag> GetTagsForProperty(int contentId, string propertyTypeAlias, string? group = null, string? culture = null)
    {
        using (ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return _tagRepository.GetTagsForProperty(contentId, propertyTypeAlias, group, culture);
        }
    }

    /// <inheritdoc />
    public IEnumerable<ITag> GetTagsForEntity(int contentId, string? group = null, string? culture = null)
    {
        using (ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return _tagRepository.GetTagsForEntity(contentId, group, culture);
        }
    }

    /// <inheritdoc />
    public IEnumerable<ITag> GetTagsForProperty(Guid contentId, string propertyTypeAlias, string? group = null, string? culture = null)
    {
        using (ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return _tagRepository.GetTagsForProperty(contentId, propertyTypeAlias, group, culture);
        }
    }

    /// <inheritdoc />
    public IEnumerable<ITag> GetTagsForEntity(Guid contentId, string? group = null, string? culture = null)
    {
        using (ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return _tagRepository.GetTagsForEntity(contentId, group, culture);
        }
    }
}
