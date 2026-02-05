using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Api.Management.ViewModels.Element;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Models.ContentPublishing;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Security.Authorization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.Element;

[ApiVersion("1.0")]
public class PublishElementController : ElementControllerBase
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IElementPublishingService _elementPublishingService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IDocumentPresentationFactory _documentPresentationFactory;

    public PublishElementController(
        IAuthorizationService authorizationService,
        IElementPublishingService elementPublishingService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IDocumentPresentationFactory documentPresentationFactory)
    {
        _authorizationService = authorizationService;
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
        AuthorizationResult authorizationResult = await _authorizationService.AuthorizeResourceAsync(
            User,
            ElementPermissionResource.WithKeys(
                ActionElementPublish.ActionLetter,
                id,
                requestModel.PublishSchedules
                    .Where(x => x.Culture is not null)
                    .Select(x => x.Culture!)),
            AuthorizationPolicies.ElementPermissionByResource);

        if (!authorizationResult.Succeeded)
        {
            return Forbidden();
        }

        // TODO ELEMENTS: IDocumentPresentationFactory carries the implementation of this mapping - it should probably be renamed
        var tempModel = new PublishDocumentRequestModel { PublishSchedules = requestModel.PublishSchedules };
        Attempt<List<CulturePublishScheduleModel>, ContentPublishingOperationStatus> modelResult = _documentPresentationFactory.CreateCulturePublishScheduleModels(tempModel);

        if (modelResult.Success is false)
        {
            return ElementPublishingOperationStatusResult(modelResult.Status);
        }

        Attempt<ContentPublishingResult, ContentPublishingOperationStatus> attempt = await _elementPublishingService.PublishAsync(
            id,
            modelResult.Result,
            CurrentUserKey(_backOfficeSecurityAccessor));
        return attempt.Success
            ? Ok()
            : ElementPublishingOperationStatusResult(attempt.Status, attempt.Result.InvalidPropertyAliases);
    }
}
