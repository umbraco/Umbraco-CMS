using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.Element.RecycleBin;

[ApiVersion("1.0")]
public class EmptyElementRecycleBinController : ElementRecycleBinControllerBase
{
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IElementContainerService _elementContainerService;

    public EmptyElementRecycleBinController(
        IEntityService entityService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IElementContainerService elementContainerService,
        IElementPresentationFactory elementPresentationFactory)
        : base(entityService, elementPresentationFactory)
    {
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _elementContainerService = elementContainerService;
    }

    [HttpDelete]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> EmptyRecycleBin(CancellationToken cancellationToken)
    {
        Attempt<EntityContainerOperationStatus> result = await _elementContainerService.EmptyRecycleBinAsync(CurrentUserKey(_backOfficeSecurityAccessor));
        return result.Success
            ? Ok()
            : OperationStatusResult(result.Result);
    }
}
