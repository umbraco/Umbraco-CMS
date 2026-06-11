using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Editors;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.HybridCache.Factories;

namespace Umbraco.Cms.Infrastructure.HybridCache.Services;

internal sealed class VisualEditorContentFactory : IVisualEditorContentFactory
{
    private readonly IIdKeyMap _idKeyMap;
    private readonly IContentService _contentService;
    private readonly IDataTypeService _dataTypeService;
    private readonly ICacheNodeFactory _cacheNodeFactory;
    private readonly IPublishedContentFactory _publishedContentFactory;
    private readonly IJsonSerializer _jsonSerializer;

    public VisualEditorContentFactory(
        IIdKeyMap idKeyMap,
        IContentService contentService,
        IDataTypeService dataTypeService,
        ICacheNodeFactory cacheNodeFactory,
        IPublishedContentFactory publishedContentFactory,
        IJsonSerializer jsonSerializer)
    {
        _idKeyMap = idKeyMap;
        _contentService = contentService;
        _dataTypeService = dataTypeService;
        _cacheNodeFactory = cacheNodeFactory;
        _publishedContentFactory = publishedContentFactory;
        _jsonSerializer = jsonSerializer;
    }

    public async Task<IPublishedContent?> CreateWithOverridesAsync(
        Guid documentKey,
        IReadOnlyCollection<VisualEditorPropertyOverride> overrides)
    {
        Attempt<int> idAttempt = _idKeyMap.GetIdForKey(documentKey, UmbracoObjectTypes.Document);
        if (idAttempt.Success is false)
        {
            return null;
        }

        IContent? content = _contentService.GetById(idAttempt.Result);
        if (content is null)
        {
            return null;
        }

        ContentCacheNode baseNode = _cacheNodeFactory.ToContentCacheNode(content, preview: true);
        if (baseNode.Data is null)
        {
            return null;
        }

        var properties = new Dictionary<string, PropertyData[]>(baseNode.Data.Properties);

        foreach (VisualEditorPropertyOverride @override in overrides)
        {
            object? source = await ConvertToSourceValueAsync(content, @override);
            properties[@override.Alias] =
            [
                new PropertyData
                {
                    Culture = @override.Culture ?? string.Empty,
                    Segment = @override.Segment ?? string.Empty,
                    Value = source,
                }
            ];
        }

#pragma warning disable CS0618 // ContentData ctor obsolete usage mirrored from the cache node source
        var overriddenNode = new ContentCacheNode
        {
            Id = baseNode.Id,
            Key = baseNode.Key,
            SortOrder = baseNode.SortOrder,
            CreateDate = baseNode.CreateDate,
            CreatorId = baseNode.CreatorId,
            ContentTypeId = baseNode.ContentTypeId,
            IsDraft = true,
            Data = new ContentData(
                name: baseNode.Data.Name,
                urlSegment: baseNode.Data.UrlSegment,
                versionId: baseNode.Data.VersionId,
                versionDate: baseNode.Data.VersionDate,
                writerId: baseNode.Data.WriterId,
                templateId: baseNode.Data.TemplateId,
                published: baseNode.Data.Published,
                properties: properties,
                cultureInfos: baseNode.Data.CultureInfos),
        };
#pragma warning restore CS0618

        return _publishedContentFactory.ToIPublishedContent(overriddenNode, preview: true);
    }

    private async Task<object?> ConvertToSourceValueAsync(IContent content, VisualEditorPropertyOverride @override)
    {
        IProperty? property = content.Properties[@override.Alias];
        if (property is null)
        {
            return null;
        }

        IDataType? dataType = await _dataTypeService.GetAsync(property.PropertyType.DataTypeKey);
        if (dataType?.Editor is null)
        {
            return null;
        }

        // FromEditor expects the serialized editor value for complex editors; a plain string passes through.
        var editorValue = @override.EditorValue is string s ? s : _jsonSerializer.Serialize(@override.EditorValue);

        return dataType.Editor.GetValueEditor().FromEditor(
            new ContentPropertyData(editorValue, dataType.ConfigurationObject),
            null);
    }
}
