using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Security.Authorization.Content;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.Document.RecycleBin;

[ApiVersion("1.0")]
public class EmptyDocumentRecycleBinController : DocumentRecycleBinControllerBase
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IContentService _contentService;
    private readonly IUserIdKeyResolver _userIdKeyResolver;

    public EmptyDocumentRecycleBinController(
        IEntityService entityService,
        IAuthorizationService authorizationService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IContentService contentService,
        IUserIdKeyResolver userIdKeyResolver)
        : base(entityService)
    {
        _authorizationService = authorizationService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _contentService = contentService;
        _userIdKeyResolver = userIdKeyResolver;
    }

    [HttpDelete]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> EmptyRecycleBin()
    {
        AuthorizationResult authorizationResult  = await _authorizationService.AuthorizeResourceAsync(
            User,
            ContentPermissionResource.RecycleBin(ActionDelete.ActionLetter),
            AuthorizationPolicies.ContentPermissionByResource);

        if (authorizationResult.Succeeded is false)
        {
            return Forbidden();
        }

        OperationResult result = _contentService.EmptyRecycleBin(await _userIdKeyResolver.GetAsync(CurrentUserKey(_backOfficeSecurityAccessor)));
        return result.Success
            ? Ok()
            : OperationStatusResult(result);
    }
}
