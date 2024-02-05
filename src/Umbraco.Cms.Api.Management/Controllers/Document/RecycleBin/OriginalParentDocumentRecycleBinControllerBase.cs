using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.Security.Authorization.Content;
using Umbraco.Cms.Api.Management.ViewModels.Document.Item;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Querying.RecycleBin;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.Document.RecycleBin;

[ApiVersion("1.0")]
public class OriginalParentDocumentRecycleBinControllerBase : DocumentRecycleBinControllerBase
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IDocumentPresentationFactory _documentPresentationFactory;
    private readonly IDocumentRecycleBinQueryService _documentRecycleBinQueryService;

    public OriginalParentDocumentRecycleBinControllerBase(
        IEntityService entityService,
        IAuthorizationService authorizationService,
        IDocumentPresentationFactory documentPresentationFactory,
        IDocumentRecycleBinQueryService documentRecycleBinQueryService)
        : base(entityService, documentPresentationFactory)
    {
        _authorizationService = authorizationService;
        _documentPresentationFactory = documentPresentationFactory;
        _documentRecycleBinQueryService = documentRecycleBinQueryService;
    }

    [HttpGet("{id:guid}/original-parent")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(DocumentItemResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> OriginalParent(Guid id)
    {
        AuthorizationResult authorizationResult = await _authorizationService.AuthorizeResourceAsync(
            User,
            ContentPermissionResource.RecycleBin(ActionRestore.ActionLetter),
            AuthorizationPolicies.ContentPermissionByResource);

        if (!authorizationResult.Succeeded)
        {
            return Forbidden();
        }

        Attempt<IDocumentEntitySlim?, RecycleBinQueryResultType> getParentAttempt = await _documentRecycleBinQueryService.GetOriginalParentAsync(id);
        if (getParentAttempt.Success is false)
        {
            return MapAttemptFailure(getParentAttempt.Status);
        }

        return getParentAttempt.Result is not null
            ? Ok(_documentPresentationFactory.CreateItemResponseModel(getParentAttempt.Result))
            : Ok(null); // map this
    }

    private IActionResult MapAttemptFailure(RecycleBinQueryResultType status)
        => OperationStatusResult(status, problemDetailsBuilder => status switch
        {
            RecycleBinQueryResultType.NotFound => NotFound(problemDetailsBuilder
                .WithTitle("The document could not be found")
                .Build()),
            RecycleBinQueryResultType.NotTrashed => BadRequest(problemDetailsBuilder
                .WithTitle("The document is not trashed")
                .WithDetail("The document needs to be trashed for the parent-before-recycled relation to be created.")
                .Build()),
            RecycleBinQueryResultType.NoParentRecycleRelation => StatusCode(StatusCodes.Status500InternalServerError, problemDetailsBuilder
                .WithTitle("The parent relation could not be found")
                .WithDetail("The relation between the parent and the document that should have been created when the document was deleted could not be found.")
                .Build()),
            _ => StatusCode(StatusCodes.Status500InternalServerError, problemDetailsBuilder
                .WithTitle("Unknown recycle bin query type.")
                .Build()),
        });
}
