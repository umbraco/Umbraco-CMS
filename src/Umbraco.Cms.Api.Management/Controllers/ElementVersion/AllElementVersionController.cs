using System.ComponentModel.DataAnnotations;
using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Element;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.ElementVersion;

[ApiVersion("1.0")]
public class AllElementVersionController : ElementVersionControllerBase
{
    private readonly IElementVersionService _elementVersionService;
    private readonly IElementVersionPresentationFactory _elementVersionPresentationFactory;

    public AllElementVersionController(
        IElementVersionService elementVersionService,
        IElementVersionPresentationFactory elementVersionPresentationFactory)
    {
        _elementVersionService = elementVersionService;
        _elementVersionPresentationFactory = elementVersionPresentationFactory;
    }

    [MapToApiVersion("1.0")]
    [HttpGet]
    [ProducesResponseType(typeof(PagedViewModel<ElementVersionItemResponseModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> All(
        CancellationToken cancellationToken,
        [Required] Guid elementId,
        string? culture,
        int skip = 0,
        int take = 100)
    {
        Attempt<PagedModel<ContentVersionMeta>?, ContentVersionOperationStatus> attempt =
            await _elementVersionService.GetPagedContentVersionsAsync(elementId, culture, skip, take);

        if (attempt.Success is false)
        {
            return MapFailure(attempt.Status);
        }

        var pagedViewModel = new PagedViewModel<ElementVersionItemResponseModel>
        {
            Total = attempt.Result!.Total,
            Items = await _elementVersionPresentationFactory.CreateMultipleAsync(attempt.Result!.Items),
        };

        return Ok(pagedViewModel);
    }
}
