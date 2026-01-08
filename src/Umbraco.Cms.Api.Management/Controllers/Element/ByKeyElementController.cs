using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Element;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.Element;

[ApiVersion("1.0")]
public class ByKeyElementController : ElementControllerBase
{
    private readonly IElementService _elementService;
    private readonly IElementPresentationFactory _elementPresentationFactory;

    public ByKeyElementController(
        IElementService elementService,
        IElementPresentationFactory elementPresentationFactory)
    {
        _elementService = elementService;
        _elementPresentationFactory = elementPresentationFactory;
    }

    [HttpGet("{id:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ElementResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ByKey(CancellationToken cancellationToken, Guid id)
    {
        IElement? element = _elementService.GetById(id);
        if (element is null)
        {
            return ContentEditingOperationStatusResult(ContentEditingOperationStatus.NotFound);
        }

        ContentScheduleCollection contentScheduleCollection = _elementService.GetContentScheduleByContentId(id);

        ElementResponseModel model = _elementPresentationFactory.CreateResponseModel(element, contentScheduleCollection);
        return Ok(model);
    }
}
