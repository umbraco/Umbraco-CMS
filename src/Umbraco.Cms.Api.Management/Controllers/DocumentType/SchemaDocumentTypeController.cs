using System.Text.Json.Nodes;
using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.DocumentType;

/// <summary>
/// Controller for retrieving document type JSON schemas.
/// </summary>
[ApiVersion("1.0")]
public class SchemaDocumentTypeController : DocumentTypeControllerBase
{
    private readonly IContentTypeJsonSchemaService _schemaService;

    /// <summary>
    /// Initializes a new instance of the <see cref="SchemaDocumentTypeController"/> class.
    /// </summary>
    public SchemaDocumentTypeController(IContentTypeJsonSchemaService schemaService)
        => _schemaService = schemaService;

    /// <summary>
    /// Gets a JSON Schema for a specific document type.
    /// </summary>
    /// <param name="id">The unique identifier of the document type.</param>
    /// <returns>A JSON Schema describing the document creation/update payload structure.</returns>
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
        JsonObject? schema = await _schemaService.GetDocumentTypeSchemaAsync(id);
        return schema is not null
            ? Ok(schema)
            : OperationStatusResult(ContentTypeOperationStatus.NotFound);
    }
}
