using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services;

/// <inheritdoc />
internal sealed class ContentTypeInputSchemaService : IContentTypeInputSchemaService
{
    private readonly IContentTypeService _contentTypeService;
    private readonly IMediaTypeService _mediaTypeService;
    private readonly IMemberTypeService _memberTypeService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ContentTypeInputSchemaService"/> class.
    /// </summary>
    public ContentTypeInputSchemaService(
        IContentTypeService contentTypeService,
        IMediaTypeService mediaTypeService,
        IMemberTypeService memberTypeService)
    {
        _contentTypeService = contentTypeService;
        _mediaTypeService = mediaTypeService;
        _memberTypeService = memberTypeService;
    }

    /// <inheritdoc />
    public Task<IReadOnlyCollection<ContentTypeInputSchema>> GetDocumentTypeSchemasAsync(IEnumerable<Guid> keys)
    {
        Guid[] keyArray = keys.ToArray();
        IEnumerable<IContentType> contentTypes = _contentTypeService.GetMany(keyArray);
        return Task.FromResult(BuildSchemaInfos(contentTypes));
    }

    /// <inheritdoc />
    public Task<IReadOnlyCollection<ContentTypeInputSchema>> GetMediaTypeSchemasAsync(IEnumerable<Guid> keys)
    {
        Guid[] keyArray = keys.ToArray();
        IEnumerable<IMediaType> mediaTypes = _mediaTypeService.GetMany(keyArray);
        return Task.FromResult(BuildSchemaInfos(mediaTypes));
    }

    /// <inheritdoc />
    public Task<IReadOnlyCollection<ContentTypeInputSchema>> GetMemberTypeSchemasAsync(IEnumerable<Guid> keys)
    {
        Guid[] keyArray = keys.ToArray();
        IEnumerable<IMemberType> memberTypes = _memberTypeService.GetMany(keyArray);
        return Task.FromResult(BuildSchemaInfos(memberTypes));
    }

    private static IReadOnlyCollection<ContentTypeInputSchema> BuildSchemaInfos<T>(IEnumerable<T> contentTypes)
        where T : IContentTypeComposition
    {
        List<ContentTypeInputSchema> results = [];

        foreach (T contentType in contentTypes)
        {
            List<PropertyInputSchema> properties = [];

            foreach (IPropertyType propertyType in contentType.CompositionPropertyTypes)
            {
                properties.Add(new PropertyInputSchema
                {
                    Alias = propertyType.Alias,
                    DataTypeKey = propertyType.DataTypeKey,
                    EditorAlias = propertyType.PropertyEditorAlias,
                    Mandatory = propertyType.Mandatory,
                    Variations = propertyType.Variations,
                });
            }

            results.Add(new ContentTypeInputSchema
            {
                Key = contentType.Key,
                Alias = contentType.Alias,
                Properties = properties,
                IsElement = contentType.IsElement,
                Variations = contentType.Variations,
            });
        }

        return results;
    }
}
