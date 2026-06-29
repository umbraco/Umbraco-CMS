using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.DataType;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.DataType;

/// <summary>
/// Controller for retrieving data type value schemas.
/// </summary>
[ApiVersion("1.0")]
public class SchemaDataTypeController : DataTypeControllerBase
{
    private readonly IPropertyEditorSchemaService _schemaService;

    /// <summary>
    /// Initializes a new instance of the <see cref="SchemaDataTypeController"/> class.
    /// </summary>
    /// <param name="schemaService">The property editor schema service.</param>
    public SchemaDataTypeController(IPropertyEditorSchemaService schemaService)
        => _schemaService = schemaService;

    /// <summary>
    /// Gets the value schema for a data type.
    /// </summary>
    /// <param name="id">The unique identifier of the data type.</param>
    /// <returns>The schema information for the data type's values.</returns>
    /// <remarks>
    /// Returns schema information for property editors that implement <c>IValueSchemaProvider</c>.
    /// Returns 404 if the data type is not found or doesn't support schema information.
    /// </remarks>
    [HttpGet("{id:guid}/schema")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(DataTypeSchemaResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Schema(Guid id)
    {
        Attempt<PropertyValueSchema, PropertyEditorSchemaOperationStatus> attempt = await _schemaService.GetSchemaAsync(id);
        if (attempt.Success is false)
        {
            return PropertyEditorSchemaOperationStatusResult(attempt.Status);
        }

        PropertyValueSchema result = attempt.Result;
        return Ok(new DataTypeSchemaResponseModel
        {
            ValueTypeName = result.ValueType?.FullName,
            JsonSchema = result.JsonSchema,
        });
    }
}
