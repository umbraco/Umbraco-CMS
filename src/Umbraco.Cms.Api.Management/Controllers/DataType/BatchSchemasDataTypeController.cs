using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.DataType;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.DataType;

/// <summary>
/// Controller for retrieving multiple data type value schemas in a single request.
/// </summary>
[ApiVersion("1.0")]
public class BatchSchemasDataTypeController : DataTypeControllerBase
{
    private readonly IPropertyEditorSchemaService _schemaService;

    /// <summary>
    /// Initializes a new instance of the <see cref="BatchSchemasDataTypeController"/> class.
    /// </summary>
    /// <param name="schemaService">The property editor schema service.</param>
    public BatchSchemasDataTypeController(IPropertyEditorSchemaService schemaService)
        => _schemaService = schemaService;

    /// <summary>
    /// Gets the value schemas for multiple data types.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <param name="ids">The unique identifiers of the data types.</param>
    /// <returns>The schema information for the requested data types.</returns>
    /// <remarks>
    /// Returns schema information for property editors that implement <c>IValueSchemaProvider</c>.
    /// Each item includes an error field if the schema could not be retrieved (e.g., data type not found or schema not supported).
    /// </remarks>
    [HttpGet("schemas/batch")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(FetchResponseModel<DataTypeSchemaItemResponseModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSchemas(
        CancellationToken cancellationToken,
        [FromQuery(Name = "id")] Guid[] ids)
    {
        Guid[] requestedIds = [.. ids.Distinct()];

        if (requestedIds.Length == 0)
        {
            return Ok(new FetchResponseModel<DataTypeSchemaItemResponseModel>());
        }

        var items = new List<DataTypeSchemaItemResponseModel>();

        foreach (Guid id in requestedIds)
        {
            Attempt<PropertyValueSchema, PropertyEditorSchemaOperationStatus> attempt = await _schemaService.GetSchemaAsync(id);
            items.Add(new DataTypeSchemaItemResponseModel
            {
                Id = id,
                ValueTypeName = attempt.Success ? attempt.Result.ValueType?.FullName : null,
                JsonSchema = attempt.Success ? attempt.Result.JsonSchema : null,
                Error = attempt.Success ? null : attempt.Status.ToString(),
            });
        }

        return Ok(new FetchResponseModel<DataTypeSchemaItemResponseModel>
        {
            Total = items.Count,
            Items = items,
        });
    }
}
