using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Api.Management.ViewModels.DataType;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;

namespace Umbraco.Cms.Api.Management.Controllers.DataType;

/// <summary>
/// Controller responsible for managing configuration for data types in the Umbraco CMS.
/// </summary>
[ApiVersion("1.0")]
public class ConfigurationDataTypeController : DataTypeControllerBase
{
    private readonly DataTypesSettings _dataTypesSettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationDataTypeController"/> class.
    /// </summary>
    /// <param name="dataTypesSettings">An <see cref="IOptionsSnapshot{T}"/> containing the <see cref="DataTypesSettings"/> configuration options.</param>
    public ConfigurationDataTypeController(IOptionsSnapshot<DataTypesSettings> dataTypesSettings) => _dataTypesSettings = dataTypesSettings.Value;

    /// <summary>
    /// Retrieves the configuration settings for data types, including whether data types can be changed and the identifiers for document and media list views.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>An <see cref="IActionResult"/> containing a <see cref="DatatypeConfigurationResponseModel"/> with the data type configuration settings.</returns>
    [HttpGet("configuration")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(DatatypeConfigurationResponseModel), StatusCodes.Status200OK)]
    [EndpointSummary("Gets the data type configuration.")]
    [EndpointDescription("Gets the configuration settings for data types.")]
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
