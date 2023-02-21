using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.Builders;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.RelationType.Query;

public class DeleteRelationTypeController : RelationTypeControllerBase
{
    private readonly IRelationService _relationService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    public DeleteRelationTypeController(IRelationService relationService, IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _relationService = relationService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    [HttpDelete("{key:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid key)
    {
        Attempt<IRelationType?, RelationTypeOperationStatus> result = await _relationService.DeleteAsync(key, CurrentUserId(_backOfficeSecurityAccessor));
        return result.Success ? await Task.FromResult(Ok()) : RelationTypeOperationStatusResult(result.Status);
    }
}
