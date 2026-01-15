using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Api.Management.ViewModels.Element;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.ContentPublishing;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.Element;

[ApiVersion("1.0")]
public class PublishElementController : ElementControllerBase
{
    private readonly IElementPublishingService _elementPublishingService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IDocumentPresentationFactory _documentPresentationFactory;

    public PublishElementController(
        IElementPublishingService elementPublishingService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IDocumentPresentationFactory documentPresentationFactory)
    {
        _elementPublishingService = elementPublishingService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _documentPresentationFactory = documentPresentationFactory;
    }

    [HttpPut("{id:guid}/publish")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Publish(CancellationToken cancellationToken, Guid id, PublishElementRequestModel requestModel)
    {
        // TODO ELEMENTS: IDocumentPresentationFactory carries the implementation of this mapping - it should probably be renamed
        var tempModel = new PublishDocumentRequestModel { PublishSchedules = requestModel.PublishSchedules };
        Attempt<List<CulturePublishScheduleModel>, ContentPublishingOperationStatus> modelResult = _documentPresentationFactory.CreateCulturePublishScheduleModels(tempModel);

        if (modelResult.Success is false)
        {
            // TODO ELEMENTS: use refactored DocumentPublishingOperationStatusResult from DocumentControllerBase once it's ready
            return BadRequest();
        }

        Attempt<ContentPublishingResult, ContentPublishingOperationStatus> attempt = await _elementPublishingService.PublishAsync(
            id,
            modelResult.Result,
            CurrentUserKey(_backOfficeSecurityAccessor));
        return attempt.Success
            ? Ok()
            // TODO ELEMENTS: use refactored DocumentPublishingOperationStatusResult from DocumentControllerBase once it's ready
            : BadRequest();
    }
}
