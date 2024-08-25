using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.PropertyType;

[ApiVersion("1.0")]
public class IsUsedPropertyTypeController : PropertyTypeControllerBase
{
    private readonly IPropertyTypeUsageService _propertyTypeUsageService;

    public IsUsedPropertyTypeController(IPropertyTypeUsageService propertyTypeUsageService)
    {
        _propertyTypeUsageService = propertyTypeUsageService;
    }

    [HttpGet("is-used")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(CancellationToken cancellationToken, Guid contentTypeId, string propertyAlias)
    {
        Attempt<bool, PropertyTypeOperationStatus> result = await _propertyTypeUsageService.HasSavedPropertyValuesAsync(contentTypeId, propertyAlias);

        return result.Success
            ? Ok(result.Result)
            : PropertyTypeOperationStatusResult(result.Status);
    }
}
