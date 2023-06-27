using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentTypeEditing.Document;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services.ContentTypeEditing;

public class DocumentTypeEditingService : IDocumentTypeEditingService
{
    private readonly IContentTypeService _contentTypeService;
    private readonly IDataTypeService _dataTypeService;

    public DocumentTypeEditingService(
        IContentTypeService contentTypeService,
        IDataTypeService dataTypeService)
    {
        _contentTypeService = contentTypeService;
        _dataTypeService = dataTypeService;
    }

    public async Task<Attempt<IContentType?, ContentTypeOperationStatus>> CreateAsync(DocumentTypeCreateModel model, Guid performingUserId)
    {
        // Validation...

        // Ensure no duplicate alias across documents, members, and media. Since this would break ModelsBuilder/published cache.
        if (_contentTypeService.GetAllContentTypeAliases().InvariantContains(model.Alias))
        {
            return Attempt.FailWithStatus<IContentType?, ContentTypeOperationStatus>(ContentTypeOperationStatus.DuplicateAlias, null);
        }

        var reservedModelAliases = new[] { "system" };
        if (reservedModelAliases.InvariantContains(model.Alias))
        {
            return Attempt.FailWithStatus<IContentType?, ContentTypeOperationStatus>(ContentTypeOperationStatus.InvalidAlias, null);
        }

        // Validate properties for reserved names.
        // Because of models builder you cannot have an alias that already exists in IPublishedContent, for instance Path.
        // Since MyModel.Path would conflict with IPublishedContent.Path.
        var reservedPropertyTypeAliases = typeof(IPublishedContent)
            .GetProperties()
            .Select(x => x.Name)
            .Union(typeof(IPublishedContent)
                .GetMethods()
                .Select(x => x.Name))
            .ToArray();

        foreach(var propertyType in model.Properties)
        {
            // The alias of the property type cannot be the same as the alias of the content type.
            if (propertyType.Alias.Equals(model.Alias, StringComparison.OrdinalIgnoreCase))
            {
                return Attempt.FailWithStatus<IContentType?, ContentTypeOperationStatus>(ContentTypeOperationStatus.InvalidPropertyTypeAlias, null);
            }

            if (reservedPropertyTypeAliases.InvariantContains(propertyType.Alias))
            {
                return Attempt.FailWithStatus<IContentType?, ContentTypeOperationStatus>(ContentTypeOperationStatus.InvalidPropertyTypeAlias, null);
            }
        }

        // Validate that all the Data types specified the property types actually exists.
        Guid[] dataTypeKeys = model.Properties.Select(property => property.DataTypeKey).ToArray();
        // We'll cache this is a dictionary, to be reused later.
        Dictionary<Guid, IDataType> dataTypesByKey = (await _dataTypeService.GetAllAsync(dataTypeKeys))
            .ToDictionary(x => x.Key);

        if (dataTypesByKey.Count != dataTypeKeys.Length)
        {
            return Attempt.FailWithStatus<IContentType?, ContentTypeOperationStatus>(ContentTypeOperationStatus.DataTypeNotFound, null);
        }

        throw new NotImplementedException();
    }
}
