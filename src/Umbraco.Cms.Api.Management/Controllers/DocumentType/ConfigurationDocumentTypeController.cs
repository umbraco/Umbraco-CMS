﻿using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Api.Management.ViewModels.DocumentType;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Features;

namespace Umbraco.Cms.Api.Management.Controllers.DocumentType;

[ApiVersion("1.0")]
public class ConfigurationDocumentTypeController : DocumentTypeControllerBase
{
    private readonly UmbracoFeatures _umbracoFeatures;
    private readonly DataTypesSettings _dataTypesSettings;

    public ConfigurationDocumentTypeController(UmbracoFeatures umbracoFeatures, IOptionsSnapshot<DataTypesSettings> dataTypesSettings)
    {
        _umbracoFeatures = umbracoFeatures;
        _dataTypesSettings = dataTypesSettings.Value;
    }

    [HttpGet("configuration")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(DocumentTypeConfigurationResponseModel), StatusCodes.Status200OK)]
    public Task<IActionResult> Configuration()
    {
        var responseModel = new DocumentTypeConfigurationResponseModel
        {
            DataTypesCanBeChanged = _dataTypesSettings.CanBeChanged,
            DisableTemplates = _umbracoFeatures.Disabled.DisableTemplates,
        };
        return Task.FromResult<IActionResult>(Ok(responseModel));
    }
}
