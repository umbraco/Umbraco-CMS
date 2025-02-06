using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Security.Authorization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.DocumentVersion;

[ApiVersion("1.0")]
public class RollbackDocumentVersionController : DocumentVersionControllerBase
{
    private readonly IContentVersionService _contentVersionService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IAuthorizationService _authorizationService;

    [ActivatorUtilitiesConstructor]
    public RollbackDocumentVersionController(
        IContentVersionService contentVersionService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IAuthorizationService authorizationService)
    {
        _contentVersionService = contentVersionService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _authorizationService = authorizationService;
    }

    // TODO (V16): Remove this constructor.
    [Obsolete("Please use the constructor taking all parameters. This constructor will be removed in V16.")]
    public RollbackDocumentVersionController(
        IContentVersionService contentVersionService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
        : this(
              contentVersionService,
              backOfficeSecurityAccessor,
              StaticServiceProvider.Instance.GetRequiredService<IAuthorizationService>())
    {
    }

    [MapToApiVersion("1.0")]
    [HttpPost("{id:guid}/rollback")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Rollback(CancellationToken cancellationToken, Guid id, string? culture)
    {
        Attempt<IContent?, ContentVersionOperationStatus> getContentAttempt =
            await _contentVersionService.GetAsync(id);
        if (getContentAttempt.Success is false || getContentAttempt.Result is null)
        {
            return MapFailure(getContentAttempt.Status);
        }

        IContent content = getContentAttempt.Result;
        AuthorizationResult authorizationResult = await _authorizationService.AuthorizeResourceAsync(
            User,
            ContentPermissionResource.WithKeys(ActionRollback.ActionLetter, content.Key),
            AuthorizationPolicies.ContentPermissionByResource);

        if (!authorizationResult.Succeeded)
        {
            return Forbidden();
        }

        Attempt<ContentVersionOperationStatus> rollBackAttempt =
            await _contentVersionService.RollBackAsync(id, culture, CurrentUserKey(_backOfficeSecurityAccessor));

        return rollBackAttempt.Success
            ? Ok()
            : MapFailure(rollBackAttempt.Result);
    }
}
