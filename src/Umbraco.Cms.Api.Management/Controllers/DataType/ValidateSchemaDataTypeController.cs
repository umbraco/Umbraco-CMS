using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.DataType;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.DataType;

/// <summary>
/// Controller for validating values against data type schemas.
/// </summary>
[ApiVersion("1.0")]
public class ValidateSchemaDataTypeController : DataTypeControllerBase
{
    private readonly IPropertyEditorSchemaService _schemaService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidateSchemaDataTypeController"/> class.
    /// </summary>
    /// <param name="schemaService">The property editor schema service.</param>
    public ValidateSchemaDataTypeController(IPropertyEditorSchemaService schemaService)
        => _schemaService = schemaService;

    /// <summary>
    /// Validates a value against the data type's JSON Schema.
    /// </summary>
    /// <param name="id">The unique identifier of the data type.</param>
    /// <param name="requestModel">The request containing the value to validate.</param>
    /// <returns>A collection of validation errors (empty if validation passes).</returns>
    /// <remarks>
    /// Returns validation results for property editors that implement <c>IValueSchemaProvider</c>.
    /// Returns 404 if the data type is not found or doesn't support schema information.
    /// </remarks>
    [HttpPost("{id:guid}/schema/validate")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<SchemaValidationResultResponseModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ValidateValue(Guid id, ValidateDataTypeValueRequestModel requestModel)
    {
        Attempt<IEnumerable<SchemaValidationResult>, PropertyEditorSchemaOperationStatus> attempt =
            await _schemaService.ValidateValueAsync(id, requestModel.Value);

        if (attempt.Success is false)
        {
            return PropertyEditorSchemaOperationStatusResult(attempt.Status);
        }

        IEnumerable<SchemaValidationResultResponseModel> results = attempt.Result
            .Select(r => new SchemaValidationResultResponseModel
            {
                Message = r.Message,
                Path = r.Path,
                Keyword = r.Keyword,
            });

        return Ok(results);
    }
}
