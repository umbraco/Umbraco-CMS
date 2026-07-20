using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Install.Models;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Infrastructure.Migrations.Install;
using Umbraco.Cms.Api.Management.ViewModels.Installer;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.Install;

/// <summary>
/// Provides API endpoints for validating the configuration and connectivity of the database during the installation process.
/// </summary>
[ApiVersion("1.0")]
public class ValidateDatabaseInstallController : InstallControllerBase
{
    private readonly DatabaseBuilder _databaseBuilder;
    private readonly IUmbracoMapper _mapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidateDatabaseInstallController"/> class.
    /// </summary>
    /// <param name="databaseBuilder">Provides functionality for setting up and validating the database.</param>
    /// <param name="mapper">Maps objects within the Umbraco context.</param>
    public ValidateDatabaseInstallController(
        DatabaseBuilder databaseBuilder,
        IUmbracoMapper mapper)
    {
        _databaseBuilder = databaseBuilder;
        _mapper = mapper;
    }

    /// <summary>
    /// Validates the provided database connection settings during the installation process.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="viewModel">The request model containing the database connection settings to validate.</param>
    /// <returns>An <see cref="IActionResult"/> indicating the result of the validation.</returns>
    [HttpPost("validate-database")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [EndpointSummary("Validates database connection.")]
    [EndpointDescription("Validates the database connection settings provided during installation.")]
    public async Task<IActionResult> ValidateDatabase(
        CancellationToken cancellationToken,
        DatabaseInstallRequestModel viewModel)
    {
        DatabaseModel databaseModel = _mapper.Map<DatabaseModel>(viewModel)!;

        Attempt<InstallOperationStatus> attempt = await _databaseBuilder.ValidateDatabaseConnectionAsync(databaseModel);


        return InstallOperationResult(attempt.Result);
    }
}
