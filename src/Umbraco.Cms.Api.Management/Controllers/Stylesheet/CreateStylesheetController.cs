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
public class CreateStylesheetController : StylesheetControllerBase
{
    private readonly IStylesheetService _stylesheetService;
    private readonly IUmbracoMapper _umbracoMapper;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    public CreateStylesheetController(
        IStylesheetService stylesheetService,
        IUmbracoMapper umbracoMapper,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _stylesheetService = stylesheetService;
        _umbracoMapper = umbracoMapper;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    [HttpPost]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create(
        CancellationToken cancellationToken,
        CreateStylesheetRequestModel requestModel)
    {
        StylesheetCreateModel createModel = _umbracoMapper.Map<StylesheetCreateModel>(requestModel)!;
        Attempt<IStylesheet?, StylesheetOperationStatus> createAttempt = await _stylesheetService.CreateAsync(createModel, CurrentUserKey(_backOfficeSecurityAccessor));

        return createAttempt.Success
            ? CreatedAtPath<ByPathStylesheetController>(controller => nameof(controller.ByPath), createAttempt.Result!.Path.SystemPathToVirtualPath())
            : StylesheetOperationStatusResult(createAttempt.Status);
    }
}
