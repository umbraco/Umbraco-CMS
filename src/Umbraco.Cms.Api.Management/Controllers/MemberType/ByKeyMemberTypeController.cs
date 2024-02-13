using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.MemberType;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.MemberType;

[ApiVersion("1.0")]
public class ByKeyMemberTypeController : MemberTypeControllerBase
{
    private readonly IMemberTypeService _memberTypeService;
    private readonly IUmbracoMapper _umbracoMapper;

    public ByKeyMemberTypeController(IMemberTypeService memberTypeService, IUmbracoMapper umbracoMapper)
    {
        _memberTypeService = memberTypeService;
        _umbracoMapper = umbracoMapper;
    }

    [HttpGet("{id:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(MemberTypeResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ByKey(Guid id)
    {
        IMemberType? MemberType = await _memberTypeService.GetAsync(id);
        if (MemberType == null)
        {
            return OperationStatusResult(ContentTypeOperationStatus.NotFound);
        }

        MemberTypeResponseModel model = _umbracoMapper.Map<MemberTypeResponseModel>(MemberType)!;
        return await Task.FromResult(Ok(model));
    }
}
