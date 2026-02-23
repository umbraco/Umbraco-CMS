using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.DocumentType;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentTypeEditing;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.ContentTypeEditing;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.DocumentType;

[ApiVersion("1.0")]
[Authorize(Policy = AuthorizationPolicies.TreeAccessDocumentTypes)]
public class UpdateDocumentTypeController : DocumentTypeControllerBase
{
    private readonly IDocumentTypeEditingPresentationFactory _documentTypeEditingPresentationFactory;
    private readonly IContentTypeEditingService _contentTypeEditingService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IContentTypeService _contentTypeService;

    public UpdateDocumentTypeController(
        IDocumentTypeEditingPresentationFactory documentTypeEditingPresentationFactory,
        IContentTypeEditingService contentTypeEditingService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IContentTypeService contentTypeService)
    {
        _documentTypeEditingPresentationFactory = documentTypeEditingPresentationFactory;
        _contentTypeEditingService = contentTypeEditingService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _contentTypeService = contentTypeService;
    }

    [HttpPut("{id:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Updates a document type.")]
    [EndpointDescription("Updates a document type identified by the provided Id with the details from the request model.")]
    public async Task<IActionResult> Update(
        CancellationToken cancellationToken,
        Guid id,
        UpdateDocumentTypeRequestModel requestModel)
    {
        IContentType? contentType = await _contentTypeService.GetAsync(id);
        if (contentType is null)
        {
            return OperationStatusResult(ContentTypeOperationStatus.NotFound);
        }

        var start = DateTime.UtcNow;
        Console.WriteLine("### Update starting at: {0}", start);
        ContentTypeUpdateModel model = _documentTypeEditingPresentationFactory.MapUpdateModel(requestModel);
        Attempt<IContentType?, ContentTypeOperationStatus> result = await _contentTypeEditingService.UpdateAsync(contentType, model, CurrentUserKey(_backOfficeSecurityAccessor));
        var finished  = DateTime.UtcNow;
        Console.WriteLine("### Update finished at: {0}", finished);
        Console.WriteLine("### Update time in seconds: {0}", (start - finished).TotalSeconds);

        return result.Success
            ? Ok()
            : OperationStatusResult(result.Status);
    }
}
