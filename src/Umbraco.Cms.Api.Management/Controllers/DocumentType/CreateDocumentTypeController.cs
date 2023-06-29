using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.DocumentType;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Core.Strings;

namespace Umbraco.Cms.Api.Management.Controllers.DocumentType;

[ApiVersion("1.0")]
public class CreateDocumentTypeController : CreateUpdateDocumentTypeControllerBase
{
    private readonly IShortStringHelper _shortStringHelper;

    public CreateDocumentTypeController(IContentTypeService contentTypeService, IDataTypeService dataTypeService, IShortStringHelper shortStringHelper, ITemplateService templateService)
        : base(contentTypeService, dataTypeService, shortStringHelper, templateService)
        => _shortStringHelper = shortStringHelper;

    [HttpPost]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(CreateDocumentTypeRequestModel requestModel)
    {
        // FIXME: support document type folders (and creation within folders)
        const int parentId = Constants.System.Root;

        IContentType contentType = new ContentType(_shortStringHelper, parentId);
        ContentTypeOperationStatus result = await HandleRequest<CreateDocumentTypeRequestModel, CreateDocumentTypePropertyTypeRequestModel, CreateDocumentTypePropertyTypeContainerRequestModel>(contentType, requestModel);

        return result == ContentTypeOperationStatus.Success
            ? CreatedAtAction<ByKeyDocumentTypeController>(controller => nameof(controller.ByKey), contentType.Key)
            : BadRequest(result);
    }
}
