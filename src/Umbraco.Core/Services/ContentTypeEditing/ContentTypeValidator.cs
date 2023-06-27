using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentTypeEditing;
using Umbraco.Cms.Core.Models.ContentTypeEditing.PropertyTypes;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services.ContentTypeEditing;

public abstract class ContentTypeValidator<TContentType, TPropertyType, TPropertyTypeContainer>
    where TContentType : ContentTypeBase<TPropertyType, TPropertyTypeContainer>
    where TPropertyType : PropertyTypeBase
    where TPropertyTypeContainer : PropertyTypeContainerBase
{
    private readonly IContentTypeService _contentTypeService;
    private readonly IDataTypeService _dataTypeService;

    protected virtual string[] ReservedContentTypeAliases { get; } = { "system" };

    public ContentTypeValidator(
        IContentTypeService contentTypeService,
        IDataTypeService dataTypeService)
    {
        _contentTypeService = contentTypeService;
        _dataTypeService = dataTypeService;
    }

    protected async Task<ContentTypeOperationStatus> ValidateCommon(TContentType contentType)
    {
        // Ensure no duplicate alias across documents, members, and media. Since this would break ModelsBuilder/published cache.
        // This this method gets aliases across documents, members, and media, so it covers it all.
        if (_contentTypeService.GetAllContentTypeAliases().InvariantContains(contentType.Alias))
        {
            return ContentTypeOperationStatus.DuplicateAlias;
        }

        if (ReservedContentTypeAliases.InvariantContains(contentType.Alias))
        {
            return ContentTypeOperationStatus.InvalidAlias;
        }

        // Validate properties for reserved names.
        // Because of contentTypes builder you cannot have an alias that already exists in IPublishedContent, for instance Path.
        // Since MyContentType.Path would conflict with IPublishedContent.Path.
        var reservedPropertyTypeAliases = typeof(IPublishedContent)
            .GetProperties()
            .Select(x => x.Name)
            .Union(typeof(IPublishedContent)
                .GetMethods()
                .Select(x => x.Name))
            .ToArray();

        foreach(TPropertyType propertyType in contentType.Properties)
        {
            // The alias of the property type cannot be the same as the alias of the content type.
            if (propertyType.Alias.Equals(contentType.Alias, StringComparison.OrdinalIgnoreCase))
            {
                return ContentTypeOperationStatus.InvalidPropertyTypeAlias;
            }

            if (reservedPropertyTypeAliases.InvariantContains(propertyType.Alias))
            {
                return ContentTypeOperationStatus.InvalidPropertyTypeAlias;
            }
        }

        // Validate that all the Data types specified the property types actually exists.
        Guid[] dataTypeKeys = contentType.Properties.Select(property => property.DataTypeKey).ToArray();
        // We'll cache this is a dictionary, to be reused later.
        // TODO: FIXME: Would be nice to re-use this later...
        Dictionary<Guid, IDataType> dataTypesByKey = (await _dataTypeService.GetAllAsync(dataTypeKeys))
            .ToDictionary(x => x.Key);

        if (dataTypesByKey.Count != dataTypeKeys.Length)
        {
            return ContentTypeOperationStatus.DataTypeNotFound;
        }

        return ContentTypeOperationStatus.Success;
    }
}
