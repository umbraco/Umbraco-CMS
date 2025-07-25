using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Api.Management.ViewModels.Element;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.Element;

[ApiVersion("1.0")]
public class ByKeyElementController : ElementControllerBase
{
    private readonly IUmbracoMapper _umbracoMapper;
    private readonly IElementService _elementService;

    public ByKeyElementController(IUmbracoMapper umbracoMapper, IElementService elementService)
    {
        _umbracoMapper = umbracoMapper;
        _elementService = elementService;
    }

    [HttpGet("{id:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(DocumentResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public Task<IActionResult> ByKey(CancellationToken cancellationToken, Guid id)
    {
        // TODO ELEMENTS: move logic to a presentation factory
        IElement? element = _elementService.GetById(id);
        if (element is null)
        {
            return Task.FromResult(ContentEditingOperationStatusResult(ContentEditingOperationStatus.NotFound));
        }

        ContentScheduleCollection contentScheduleCollection = _elementService.GetContentScheduleByContentId(id);

        var model = new ElementResponseModel();
        _umbracoMapper.Map(element, model);
        _umbracoMapper.Map(contentScheduleCollection, model);

        return Task.FromResult<IActionResult>(Ok(model));
    }
}
