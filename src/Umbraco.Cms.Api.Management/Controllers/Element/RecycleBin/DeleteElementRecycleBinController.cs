using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.Element.RecycleBin;

[ApiVersion("1.0")]
public class DeleteElementRecycleBinController : ElementRecycleBinControllerBase
{
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IElementEditingService _elementEditingService;

    public DeleteElementRecycleBinController(
        IEntityService entityService,
        IElementPresentationFactory elementPresentationFactory,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IElementEditingService elementEditingService)
        : base(entityService, elementPresentationFactory)
    {
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _elementEditingService = elementEditingService;
    }

    [HttpDelete("{id:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(CancellationToken cancellationToken, Guid id)
    {
        Attempt<IElement?, ContentEditingOperationStatus> result = await _elementEditingService.DeleteFromRecycleBinAsync(id, CurrentUserKey(_backOfficeSecurityAccessor));

        return result.Success
            ? Ok()
            : ContentEditingOperationStatusResult(result.Status);
    }
}
