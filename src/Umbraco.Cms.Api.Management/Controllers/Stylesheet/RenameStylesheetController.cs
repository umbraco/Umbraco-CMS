using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Extensions;
using Umbraco.Cms.Api.Management.ViewModels.Stylesheet;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.Stylesheet;

[ApiVersion("1.0")]
[Authorize(Policy = AuthorizationPolicies.TreeAccessStylesheets)]
public class RenameStylesheetController : StylesheetControllerBase
{
    private readonly IStylesheetService _stylesheetService;
    private readonly IUmbracoMapper _umbracoMapper;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    public RenameStylesheetController(IStylesheetService stylesheetService, IBackOfficeSecurityAccessor backOfficeSecurityAccessor, IUmbracoMapper umbracoMapper)
    {
        _stylesheetService = stylesheetService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _umbracoMapper = umbracoMapper;
    }

    [HttpPut("{path}/rename")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Rename(
        CancellationToken cancellationToken,
        string path,
        RenameStylesheetRequestModel requestModel)
    {
        StylesheetRenameModel renameModel = _umbracoMapper.Map<StylesheetRenameModel>(requestModel)!;

        path = DecodePath(path).VirtualPathToSystemPath();
        Attempt<IStylesheet?, StylesheetOperationStatus> renameAttempt = await _stylesheetService.RenameAsync(path, renameModel, CurrentUserKey(_backOfficeSecurityAccessor));

        return renameAttempt.Success
            ? CreatedAtPath<ByPathStylesheetController>(controller => nameof(controller.ByPath), renameAttempt.Result!.Path.SystemPathToVirtualPath())
            : StylesheetOperationStatusResult(renameAttempt.Status);
    }
}
