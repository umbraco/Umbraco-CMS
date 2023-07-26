using Umbraco.Cms.Api.Management.ViewModels.ContentType;
using Umbraco.Cms.Api.Management.ViewModels.DocumentType;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;
using ContentTypeSort = Umbraco.Cms.Core.Models.ContentTypeSort;

namespace Umbraco.Cms.Api.Management.Controllers.DocumentType;

// FIXME: pretty much everything here should be moved to mappers and possibly new services for content type editing - like the ContentEditingService for content
public abstract class CreateUpdateDocumentTypeControllerBase : DocumentTypeControllerBase
{
    private readonly IContentTypeService _contentTypeService;
    private readonly IDataTypeService _dataTypeService;
    private readonly IShortStringHelper _shortStringHelper;
    private readonly ITemplateService _templateService;
    private readonly IEntityService _entityService;
    private const int MaxInheritance = 1;

    protected CreateUpdateDocumentTypeControllerBase(
        IContentTypeService contentTypeService,
        IDataTypeService dataTypeService,
        IShortStringHelper shortStringHelper,
        ITemplateService templateService,
        IEntityService entityService)
    {
        _contentTypeService = contentTypeService;
        _dataTypeService = dataTypeService;
        _shortStringHelper = shortStringHelper;
        _templateService = templateService;
        _entityService = entityService;
    }

    protected async Task<ContentTypeOperationStatus> HandleRequest<TRequestModel, TPropertyType, TPropertyTypeContainer>(IContentType contentType, TRequestModel requestModel)
        where TRequestModel : ContentTypeModelBase<TPropertyType, TPropertyTypeContainer>, IDocumentTypeRequestModel
        where TPropertyType : PropertyTypeModelBase
        where TPropertyTypeContainer : PropertyTypeContainerModelBase
    {
        // TODO: inject and use content editing service
        return await Task.FromResult(ContentTypeOperationStatus.NotFound);
    }
}
