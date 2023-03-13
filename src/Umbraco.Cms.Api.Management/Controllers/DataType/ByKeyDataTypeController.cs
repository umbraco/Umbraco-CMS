﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.DataType;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.DataType;

public class ByKeyDataTypeController : DataTypeControllerBase
{
    private readonly IDataTypeService _dataTypeService;
    private readonly IUmbracoMapper _umbracoMapper;

    public ByKeyDataTypeController(IDataTypeService dataTypeService, IUmbracoMapper umbracoMapper)
    {
        _dataTypeService = dataTypeService;
        _umbracoMapper = umbracoMapper;
    }

    [HttpGet("{key:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(DataTypeResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DataTypeResponseModel>> ByKey(Guid key)
    {
        IDataType? dataType = await _dataTypeService.GetAsync(key);
        if (dataType == null)
        {
            return NotFound();
        }

        return Ok(_umbracoMapper.Map<DataTypeResponseModel>(dataType));
    }
}
