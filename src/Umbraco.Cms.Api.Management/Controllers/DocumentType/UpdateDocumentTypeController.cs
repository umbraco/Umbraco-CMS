using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.DocumentType;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Core.Strings;

namespace Umbraco.Cms.Api.Management.Controllers.DocumentType;

[ApiVersion("1.0")]
public class UpdateDocumentTypeController : CreateUpdateDocumentTypeControllerBase
{
    private readonly IContentTypeService _contentTypeService;

    public UpdateDocumentTypeController(
        IContentTypeService contentTypeService,
        IDataTypeService dataTypeService,
        IShortStringHelper shortStringHelper,
        ITemplateService templateService,
        IEntityService entityService)
        : base(contentTypeService, dataTypeService, shortStringHelper, templateService, entityService)
        => _contentTypeService = contentTypeService;

    [HttpPut("{id:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, UpdateDocumentTypeRequestModel requestModel)
    {
        if (requestModel.Compositions.Any())
        {
            return await Task.FromResult(BadRequest("Compositions and inheritance is not yet supported by this endpoint"));
        }

        IContentType? contentType = _contentTypeService.Get(id);
        if (contentType is null)
        {
            return NotFound();
        }

        ContentTypeOperationStatus result = await HandleRequest<UpdateDocumentTypeRequestModel, UpdateDocumentTypePropertyTypeRequestModel, UpdateDocumentTypePropertyTypeContainerRequestModel>(contentType, requestModel);

        return result == ContentTypeOperationStatus.Success
            ? Ok()
            : BadRequest(result);
    }
}
