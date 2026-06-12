using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Editors;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.HybridCache.Factories;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.HybridCache.Services;

internal sealed class VisualEditorContentFactory : IVisualEditorContentFactory
{
    private readonly IIdKeyMap _idKeyMap;
    private readonly IContentService _contentService;
    private readonly IDataTypeService _dataTypeService;
    private readonly ICacheNodeFactory _cacheNodeFactory;
    private readonly IPublishedContentFactory _publishedContentFactory;
    private readonly IJsonSerializer _jsonSerializer;
    private readonly IPublishedModelFactory _publishedModelFactory;

    public VisualEditorContentFactory(
        IIdKeyMap idKeyMap,
        IContentService contentService,
        IDataTypeService dataTypeService,
        ICacheNodeFactory cacheNodeFactory,
        IPublishedContentFactory publishedContentFactory,
        IJsonSerializer jsonSerializer,
        IPublishedModelFactory publishedModelFactory)
    {
        _idKeyMap = idKeyMap;
        _contentService = contentService;
        _dataTypeService = dataTypeService;
        _cacheNodeFactory = cacheNodeFactory;
        _publishedContentFactory = publishedContentFactory;
        _jsonSerializer = jsonSerializer;
        _publishedModelFactory = publishedModelFactory;
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
            (IDataValueEditor valueEditor, object? configuration)? resolved = await ResolveEditorAsync(content, @override);
            if (resolved is null)
            {
                continue;
            }

            var editorValue = @override.EditorValue is string s ? s : _jsonSerializer.Serialize(@override.EditorValue);
            object? source = resolved.Value.valueEditor.FromEditor(
                new ContentPropertyData(editorValue, resolved.Value.configuration),
                null);

            string overrideCulture = @override.Culture ?? string.Empty;
            string overrideSegment = @override.Segment ?? string.Empty;

            var overrideEntry = new PropertyData
            {
                Culture = overrideCulture,
                Segment = overrideSegment,
                Value = source,
            };

            if (properties.TryGetValue(@override.Alias, out PropertyData[]? existing))
            {
                PropertyData[] merged = existing
                    .Where(p => !(p.Culture == overrideCulture && p.Segment == overrideSegment))
                    .Append(overrideEntry)
                    .ToArray();
                properties[@override.Alias] = merged;
            }
            else
            {
                properties[@override.Alias] = [overrideEntry];
            }
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

        return _publishedContentFactory.ToIPublishedContent(overriddenNode, preview: true).CreateModel(_publishedModelFactory);
    }

    private async Task<(IDataValueEditor, object? configuration)?> ResolveEditorAsync(IContent content, VisualEditorPropertyOverride @override)
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

        return (dataType.Editor.GetValueEditor(), dataType.ConfigurationObject);
    }
}
