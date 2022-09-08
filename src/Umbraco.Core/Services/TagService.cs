using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Tag service to query for tags in the tags db table. The tags returned are only relevant for published content &
///     saved media or members
/// </summary>
/// <remarks>
///     If there is unpublished content with tags, those tags will not be contained
/// </remarks>
public class TagService : RepositoryService, ITagService
{
    private readonly ITagRepository _tagRepository;

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
