using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Search.Core.Extensions;
using Umbraco.Cms.Search.Core.Helpers;
using Umbraco.Cms.Search.Core.Models.Indexing;

namespace Umbraco.Cms.Search.Core.Services.ContentIndexing.Indexers;

internal sealed class SystemFieldsContentIndexer : ISystemFieldsContentIndexer
{
    private readonly IIdKeyMap _idKeyMap;
    private readonly ITagService _tagService;
    private readonly IDateTimeOffsetConverter _dateTimeOffsetConverter;
    private readonly ILogger<SystemFieldsContentIndexer> _logger;

    public SystemFieldsContentIndexer(
        IIdKeyMap idKeyMap,
        ITagService tagService,
        IDateTimeOffsetConverter dateTimeOffsetConverter,
        ILogger<SystemFieldsContentIndexer> logger)
    {
        _idKeyMap = idKeyMap;
        _tagService = tagService;
        _dateTimeOffsetConverter = dateTimeOffsetConverter;
        _logger = logger;
    }

    public Task<IEnumerable<IndexField>> GetIndexFieldsAsync(IContentBase content, string?[] cultures, bool published, CancellationToken cancellationToken)
        => Task.FromResult(CollectSystemFields(content, cultures));

    private IEnumerable<IndexField> CollectSystemFields(IContentBase content, string?[] cultures)
    {
        UmbracoObjectTypes objectType = content.ObjectType();
        if (TryGetParentId(content, objectType, out Guid? parentKey) is false)
        {
            return [];
        }

        if (TryGetPathIds(content, objectType, out IList<Guid> pathKeys) is false)
        {
            return [];
        }

        var fields = new List<IndexField>
        {
            new(Constants.FieldNames.Id, new() { Keywords = [content.Key.AsKeyword()] }, null, null),
            new(Constants.FieldNames.ParentId, new() { Keywords = [parentKey.Value.AsKeyword()] }, null, null),
            new(Constants.FieldNames.PathIds, new() { Keywords = pathKeys.Select(key => key.AsKeyword()).ToArray() }, null, null),
            new(Constants.FieldNames.ContentTypeId, new() { Keywords = [content.ContentType.Key.AsKeyword()] }, null, null),
            new(Constants.FieldNames.CreateDate, new() { DateTimeOffsets = [_dateTimeOffsetConverter.ToDateTimeOffset(content.CreateDate)] }, null, null),
            new(Constants.FieldNames.UpdateDate, new() { DateTimeOffsets = [_dateTimeOffsetConverter.ToDateTimeOffset(content.UpdateDate)] }, null, null),
            new(Constants.FieldNames.Level, new() { Integers = [content.Level] }, null, null),
            new(Constants.FieldNames.ObjectType, new() { Keywords = [objectType.ToString()] }, null, null),
            new(Constants.FieldNames.SortOrder, new() { Integers = [content.SortOrder] }, null, null),
        };

        fields.AddRange(GetCultureTagFields(content, cultures));
        fields.AddRange(GetCultureNameFields(content, cultures));

        return fields;
    }

    private bool TryGetParentId(IContentBase content, UmbracoObjectTypes objectType, [NotNullWhen(true)] out Guid? parentId)
    {
        if (content.ParentId <= 0)
        {
            if (content.Trashed)
            {
                Guid? recycleBinId = GetRecycleBinId(objectType);
                if (recycleBinId.HasValue is false)
                {
                    _logger.LogWarning(
                        "Could not resolve recycle bin key as parent key for object type {objectType} - aborting indexing of content item {contentKey}.",
                        objectType,
                        content.Key);
                    parentId = null;
                    return false;
                }

                parentId = recycleBinId;
            }
            else
            {
                // empty GUID means "root of tree, not trashed"
                parentId = Guid.Empty;
            }

            return true;
        }

        Attempt<Guid> parentKeyAttempt = _idKeyMap.GetKeyForId(content.ParentId, objectType);
        if (parentKeyAttempt.Success is false)
        {
            _logger.LogWarning(
                "Could not resolve parent key for parent ID {parentId} - aborting indexing of content item {contentKey}.",
                content.ParentId,
                content.Key);
            parentId = null;
            return false;
        }

        parentId = parentKeyAttempt.Result;
        return true;
    }

    private bool TryGetPathIds(IContentBase content, UmbracoObjectTypes objectType, out IList<Guid> pathIds)
    {
        pathIds = new List<Guid>();

        if (content.Trashed)
        {
            Guid? recycleBinId = GetRecycleBinId(objectType);
            if (recycleBinId.HasValue is false)
            {
                _logger.LogWarning(
                    "Could not resolve recycle bin key for object type {objectType} - aborting indexing of content item {contentKey}.",
                    objectType,
                    content.Key);
                return false;
            }
            pathIds.Add(recycleBinId.Value);
        }

        IEnumerable<int> ancestorIds = content.AncestorIds();
        foreach (var ancestorId in ancestorIds)
        {
            Attempt<Guid> attempt = _idKeyMap.GetKeyForId(ancestorId, objectType);
            if (attempt.Success is false)
            {
                _logger.LogWarning(
                    "Could not resolve ancestor key for ancestor ID {ancestorId} - aborting indexing of content item {contentKey}.",
                    ancestorId,
                    content.Key);
                return false;
            }

            pathIds.Add(attempt.Result);
        }

        pathIds.Add(content.Key);
        return true;
    }

    private IEnumerable<IndexField> GetCultureTagFields(IContentBase content, string?[] cultures)
    {
        foreach (var culture in cultures)
        {
            var tags = _tagService
                .GetTagsForEntity(content.Key, group: null, culture: culture)
                .Select(tag => tag.Text)
                .ToArray();
            if (tags.Length == 0)
            {
                continue;
            }

            yield return new IndexField(Constants.FieldNames.Tags, new() { Keywords = tags }, culture, null);
        }
    }

    private IEnumerable<IndexField> GetCultureNameFields(IContentBase content, string?[] cultures)
    {
        foreach (var culture in cultures)
        {
            var name = content.GetCultureName(culture);
            if (string.IsNullOrEmpty(name))
            {
                _logger.LogWarning(
                    "Could not obtain a name for indexing for content item {contentKey} in culture {culture}.",
                    content.Key,
                    culture ?? "[invariant]");
                continue;
            }

            yield return new IndexField(Constants.FieldNames.Name, new() { TextsR1 = [name] }, culture, null);
        }
    }

    private Guid? GetRecycleBinId(UmbracoObjectTypes objectType)
        => objectType is UmbracoObjectTypes.Document
            ? Cms.Core.Constants.System.RecycleBinContentKey
            : objectType is UmbracoObjectTypes.Media
                ? Cms.Core.Constants.System.RecycleBinMediaKey
                : null;
}
