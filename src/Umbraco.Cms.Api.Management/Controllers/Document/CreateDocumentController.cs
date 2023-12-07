using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.Security.Authorization.Content;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.Document;

[ApiVersion("1.0")]
public class CreateDocumentController : DocumentControllerBase
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IDocumentEditingPresentationFactory _documentEditingPresentationFactory;
    private readonly IContentEditingService _contentEditingService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    public CreateDocumentController(
        IAuthorizationService authorizationService,
        IDocumentEditingPresentationFactory documentEditingPresentationFactory,
        IContentEditingService contentEditingService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _authorizationService = authorizationService;
        _documentEditingPresentationFactory = documentEditingPresentationFactory;
        _contentEditingService = contentEditingService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    [HttpPost]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create(CreateDocumentRequestModel requestModel)
    {
        AuthorizationResult authorizationResult;

        if (requestModel.ParentId.HasValue is false)
        {
            authorizationResult = await _authorizationService.AuthorizeAsync(User, new[] { ActionNew.ActionLetter },
                $"New{AuthorizationPolicies.ContentPermissionAtRoot}");
        }
        else
        {
            var resource = new ContentPermissionResource(requestModel.ParentId.Value, ActionNew.ActionLetter);
            authorizationResult = await _authorizationService.AuthorizeResourceAsync(User, resource, AuthorizationPolicies.ContentPermissionByResource);
        }

        if (!authorizationResult.Succeeded)
        {
            return Forbidden();
        }

        ContentCreateModel model = _documentEditingPresentationFactory.MapCreateModel(requestModel);
        Attempt<IContent?, ContentEditingOperationStatus> result = await _contentEditingService.CreateAsync(model, CurrentUserKey(_backOfficeSecurityAccessor));

        return result.Success
            ? CreatedAtAction<ByKeyDocumentController>(controller => nameof(controller.ByKey), result.Result!.Key)
            : ContentEditingOperationStatusResult(result.Status);
    }
}
