using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.PropertyType;

/// <summary>
/// Controller responsible for determining whether a specific property type is currently in use.
/// </summary>
[ApiVersion("1.0")]
public class IsUsedPropertyTypeController : PropertyTypeControllerBase
{
    private readonly IPropertyTypeUsageService _propertyTypeUsageService;

    /// <summary>
    /// Initializes a new instance of the <see cref="IsUsedPropertyTypeController"/> class.
    /// </summary>
    /// <param name="propertyTypeUsageService">
    /// The <see cref="IPropertyTypeUsageService"/> used to check if a property type is in use.
    /// </param>
    public IsUsedPropertyTypeController(IPropertyTypeUsageService propertyTypeUsageService)
    {
        _propertyTypeUsageService = propertyTypeUsageService;
    }

    /// <summary>
    /// Determines whether the property type specified by the given content type ID and property alias is used in any content, media, or member items.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="contentTypeId">The unique identifier of the content type.</param>
    /// <param name="propertyAlias">The alias of the property type to check.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an <see cref="IActionResult"/> with a boolean value indicating whether the property type is used (<c>true</c>) or not (<c>false</c>).</returns>
    [HttpGet("is-used")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [EndpointSummary("Checks if a property type is used.")]
    [EndpointDescription("Checks if the property type identified by the provided content type id and property alias is used in any content, media, or members.")]
    public async Task<IActionResult> Get(CancellationToken cancellationToken, Guid contentTypeId, string propertyAlias)
    {
        Attempt<bool, PropertyTypeOperationStatus> result = await _propertyTypeUsageService.HasSavedPropertyValuesAsync(contentTypeId, propertyAlias);

        return result.Success
            ? Ok(result.Result)
            : PropertyTypeOperationStatusResult(result.Status);
    }
}
