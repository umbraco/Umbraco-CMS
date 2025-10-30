﻿using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Api.Management.ViewModels.DataType;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Management.Controllers.DataType;

[ApiVersion("1.0")]
public class ConfigurationDataTypeController : DataTypeControllerBase
{
    private readonly DataTypesSettings _dataTypesSettings;

    public ConfigurationDataTypeController(IOptionsSnapshot<DataTypesSettings> dataTypesSettings) => _dataTypesSettings = dataTypesSettings.Value;

    [HttpGet("configuration")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(DatatypeConfigurationResponseModel), StatusCodes.Status200OK)]
    [EndpointSummary("Gets the datatype configuration.")]
    [EndpointDescription("Gets the configuration settings for datatypes.")]
    public Task<IActionResult> Configuration(CancellationToken cancellationToken)
    {
        var responseModel = new DatatypeConfigurationResponseModel
        {
            CanBeChanged = _dataTypesSettings.CanBeChanged,
            DocumentListViewId = Constants.DataTypes.Guids.ListViewContentGuid,
            MediaListViewId = Constants.DataTypes.Guids.ListViewMediaGuid,
        };
        return Task.FromResult<IActionResult>(Ok(responseModel));
    }
}
