﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.MemberType.Items;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.MemberType.Items;

public class ItemMemberTypeItemController : MemberTypeItemControllerBase
{
    private readonly IUmbracoMapper _mapper;
    private readonly IMemberTypeService _memberTypeService;

    public ItemMemberTypeItemController(IUmbracoMapper mapper, IMemberTypeService memberTypeService)
    {
        _mapper = mapper;
        _memberTypeService = memberTypeService;
    }

    [HttpGet("item")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<MemberTypeItemResponseModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Item([FromQuery(Name = "id")] SortedSet<Guid> ids)
    {
        IEnumerable<IMemberType> memberTypes = _memberTypeService.GetAll(ids);
        List<MemberTypeItemResponseModel> responseModels = _mapper.MapEnumerable<IMemberType, MemberTypeItemResponseModel>(memberTypes);
        return Ok(responseModels);
    }
}
