using System.Text.Json.Nodes;
using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.MediaType;

/// <summary>
/// Controller for retrieving media type JSON schemas.
/// </summary>
[ApiVersion("1.0")]
public class SchemaMediaTypeController : MediaTypeControllerBase
{
    private readonly IContentTypeJsonSchemaService _schemaService;

    /// <summary>
    /// Initializes a new instance of the <see cref="SchemaMediaTypeController"/> class.
    /// </summary>
    public SchemaMediaTypeController(IContentTypeJsonSchemaService schemaService)
        => _schemaService = schemaService;

    /// <summary>
    /// Gets a JSON Schema for a specific media type.
    /// </summary>
    /// <param name="id">The unique identifier of the media type.</param>
    /// <returns>A JSON Schema describing the media creation/update payload structure.</returns>
    /// <remarks>
    /// The returned JSON Schema references data type schemas via external <c>$ref</c> URIs.
    /// Tooling should resolve these references by making HTTP requests to the data type schema endpoints.
    /// </remarks>
    [HttpGet("{id:guid}/schema")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(JsonObject), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSchema(Guid id)
    {
        JsonObject? schema = await _schemaService.GetMediaTypeSchemaAsync(id);
        return schema is not null
            ? Ok(schema)
            : OperationStatusResult(ContentTypeOperationStatus.NotFound);
    }
}
