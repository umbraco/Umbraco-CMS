using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.PartialView;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.PartialView;

public class CreatePartialViewController : PartialViewControllerBase
{
    private readonly IUmbracoMapper _umbracoMapper;
    private readonly IPartialViewService _partialViewService;

    public CreatePartialViewController(
        IUmbracoMapper umbracoMapper,
        IPartialViewService partialViewService)
    {
        _umbracoMapper = umbracoMapper;
        _partialViewService = partialViewService;
    }

    [HttpPost]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> Create(CreatePartialViewRequestModel createRequestModel)
    {
        PartialViewCreateModel createModel = _umbracoMapper.Map<PartialViewCreateModel>(createRequestModel)!;
    }
}
