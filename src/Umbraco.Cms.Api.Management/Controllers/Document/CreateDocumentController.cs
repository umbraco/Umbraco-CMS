﻿using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.Document;

[ApiVersion("1.0")]
public class CreateDocumentController : CreateDocumentControllerBase
{
    private readonly IDocumentEditingPresentationFactory _documentEditingPresentationFactory;
    private readonly IContentEditingService _contentEditingService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    public CreateDocumentController(
        IAuthorizationService authorizationService,
        IDocumentEditingPresentationFactory documentEditingPresentationFactory,
        IContentEditingService contentEditingService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
        : base(authorizationService)
    {
        _documentEditingPresentationFactory = documentEditingPresentationFactory;
        _contentEditingService = contentEditingService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    [HttpPost]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create(CreateDocumentRequestModel requestModel)
        => await HandleRequest(requestModel.ParentId, async () =>
    {
        AuthorizationResult authorizationResult  = await _authorizationService.AuthorizeResourceAsync(
            User,
            ContentPermissionResource.WithKeys(
                ActionNew.ActionLetter,
                requestModel.ParentId,
                requestModel.Variants
                    .Where(v => v.Culture is not null)
                    .Select(v => v.Culture!)),
            AuthorizationPolicies.ContentPermissionByResource);

        if (!authorizationResult.Succeeded)
        {
            ContentCreateModel model = _documentEditingPresentationFactory.MapCreateModel(requestModel);
            Attempt<ContentCreateResult, ContentEditingOperationStatus> result =
                await _contentEditingService.CreateAsync(model, CurrentUserKey(_backOfficeSecurityAccessor));

            return result.Success
                ? CreatedAtId<ByKeyDocumentController>(controller => nameof(controller.ByKey), result.Result.Content!.Key)
                : ContentEditingOperationStatusResult(result.Status);
        });
}
