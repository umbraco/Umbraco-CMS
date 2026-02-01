using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Factories;

internal sealed class DocumentEditingPresentationFactory : ContentEditingPresentationFactory<DocumentValueModel, DocumentVariantRequestModel>, IDocumentEditingPresentationFactory
{
    private readonly PropertyEditorCollection _propertyEditorCollection;
    private readonly IDataValueEditorFactory _dataValueEditorFactory;
    private readonly ITemplateService _templateService;

    public DocumentEditingPresentationFactory(
        PropertyEditorCollection propertyEditorCollection,
        IDataValueEditorFactory dataValueEditorFactory,
        ITemplateService templateService)
    {
        _propertyEditorCollection = propertyEditorCollection;
        _dataValueEditorFactory = dataValueEditorFactory;
        _templateService = templateService;
    }

    public ContentCreateModel MapCreateModel(CreateDocumentRequestModel requestModel)
    {
        ContentCreateModel model = MapContentEditingModel<ContentCreateModel>(requestModel);
        model.Key = requestModel.Id;
        model.ContentTypeKey = requestModel.DocumentType.Id;
        model.TemplateKey = requestModel.Template?.Id;
        model.ParentKey = requestModel.Parent?.Id;

        return model;
    }

    public ContentUpdateModel MapUpdateModel(UpdateDocumentRequestModel requestModel)
        => MapUpdateContentModel<ContentUpdateModel>(requestModel);

    public async Task<UpdateDocumentRequestModel> CreateUpdateRequestModelAsync(IContent content)
    {
        // Map values (similar to ContentMapDefinition.MapValueViewModels)
        var values = MapValuesToRequestModel(content.Properties);

        // Map variants (culture/segment with name)
        var variants = MapVariantsToRequestModel(content);

        // Map template
        Guid? templateKey = content.TemplateId.HasValue
            ? (await _templateService.GetAsync(content.TemplateId.Value))?.Key
            : null;

        return new UpdateDocumentRequestModel
        {
            Values = values,
            Variants = variants,
            Template = templateKey.HasValue ? new ReferenceByIdModel { Id = templateKey.Value } : null
        };
    }

    private DocumentValueModel[] MapValuesToRequestModel(IPropertyCollection properties)
    {
        Dictionary<string, IDataEditor> missingPropertyEditors = [];
        return properties
            .SelectMany(property => property
                .Values
                .Select(propertyValue =>
                {
                    IDataEditor? propertyEditor = _propertyEditorCollection[property.PropertyType.PropertyEditorAlias];
                    if (propertyEditor is null && !missingPropertyEditors.TryGetValue(property.PropertyType.PropertyEditorAlias, out propertyEditor))
                    {
                        // Cache missing property editors to avoid creating multiple instances
                        propertyEditor = new MissingPropertyEditor(property.PropertyType.PropertyEditorAlias, _dataValueEditorFactory);
                        missingPropertyEditors[property.PropertyType.PropertyEditorAlias] = propertyEditor;
                    }

                    return new DocumentValueModel
                    {
                        Culture = propertyValue.Culture,
                        Segment = propertyValue.Segment,
                        Alias = property.Alias,
                        Value = propertyEditor.GetValueEditor().ToEditor(property, propertyValue.Culture, propertyValue.Segment),
                    };
                }))
            .WhereNotNull()
            .ToArray();
    }

    private DocumentVariantRequestModel[] MapVariantsToRequestModel(IContent content)
    {
        IPropertyValue[] propertyValues = content.Properties.SelectMany(propertyCollection => propertyCollection.Values).ToArray();
        var cultures = content.AvailableCultures.DefaultIfEmpty(null).ToArray();
        // The default segment (null) must always be included
        var segments = propertyValues.Select(property => property.Segment).Union([null]).Distinct().ToArray();

        return cultures
            .SelectMany(culture => segments.Select(segment => new DocumentVariantRequestModel
            {
                Culture = culture,
                Segment = segment,
                Name = content.GetCultureName(culture) ?? string.Empty,
            }))
            .ToArray();
    }

    public ContentPatchModel MapPatchModel(PatchDocumentRequestModel requestModel)
    {
        var cultureExtractor = new Umbraco.Cms.Core.PropertyEditors.JsonPath.JsonPathCultureExtractor();
        var operations = requestModel.Operations.Select(op => new PatchOperationModel
        {
            Op = MapOperationType(op.Op),
            Path = op.Path,
            Value = op.Value
        }).ToArray();

        var affectedCultures = cultureExtractor.ExtractCulturesFromOperations(operations.Select(o => o.Path)).ToArray();
        var affectedSegments = operations
            .SelectMany(o => cultureExtractor.ExtractSegments(o.Path))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return new ContentPatchModel
        {
            Operations = operations,
            AffectedCultures = affectedCultures,
            AffectedSegments = affectedSegments
        };
    }

    private static PatchOperationType MapOperationType(string op)
    {
        return op.ToLowerInvariant() switch
        {
            "replace" => PatchOperationType.Replace,
            "add" => PatchOperationType.Add,
            "remove" => PatchOperationType.Remove,
            _ => throw new ArgumentException($"Unsupported operation type: {op}", nameof(op))
        };
    }

    public ValidateContentUpdateModel MapValidateUpdateModel(ValidateUpdateDocumentRequestModel requestModel)
    {
        ValidateContentUpdateModel model = MapUpdateContentModel<ValidateContentUpdateModel>(requestModel);
        model.Cultures = requestModel.Cultures;

        return model;
    }

    private TUpdateModel MapUpdateContentModel<TUpdateModel>(UpdateDocumentRequestModel requestModel)
        where TUpdateModel : ContentUpdateModel, new()
    {
        TUpdateModel model = MapContentEditingModel<TUpdateModel>(requestModel);
        model.TemplateKey = requestModel.Template?.Id;

        return model;
    }
}
