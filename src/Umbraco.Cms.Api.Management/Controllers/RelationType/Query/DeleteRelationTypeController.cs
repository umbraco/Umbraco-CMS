using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.RelationType.Query;

[ApiVersion("1.0")]
public class DeleteRelationTypeController : RelationTypeControllerBase
{
    private readonly IRelationService _relationService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    public DeleteRelationTypeController(IRelationService relationService, IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _relationService = relationService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    [HttpDelete("{id:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        Attempt<IRelationType?, RelationTypeOperationStatus> result = await _relationService.DeleteAsync(id, CurrentUserKey(_backOfficeSecurityAccessor));
        return result.Success ? await Task.FromResult(Ok()) : RelationTypeOperationStatusResult(result.Status);
    }
}
