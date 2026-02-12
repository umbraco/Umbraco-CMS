using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.ContentType;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.MemberType;

/// <summary>
/// Controller for retrieving member type input schemas.
/// </summary>
[ApiVersion("1.0")]
public class InputSchemaMemberTypeController : MemberTypeControllerBase
{
    private readonly IContentTypeInputSchemaService _inputSchemaService;

    /// <summary>
    /// Initializes a new instance of the <see cref="InputSchemaMemberTypeController"/> class.
    /// </summary>
    public InputSchemaMemberTypeController(IContentTypeInputSchemaService inputSchemaService)
        => _inputSchemaService = inputSchemaService;

    /// <summary>
    /// Gets input schemas for specific member types.
    /// </summary>
    /// <param name="keys">The keys of the member types to retrieve schemas for.</param>
    /// <returns>The input schema information for the requested member types.</returns>
    [HttpGet("schema")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<ContentTypeInputSchemaResponseModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInputSchemas([FromQuery(Name = "key")] IEnumerable<Guid> keys)
    {
        IReadOnlyCollection<ContentTypeInputSchema> schemas = await _inputSchemaService.GetMemberTypeSchemasAsync(keys);
        return Ok(schemas.Select(MapToResponseModel));
    }

    private static ContentTypeInputSchemaResponseModel MapToResponseModel(ContentTypeInputSchema schema)
        => new()
        {
            Id = schema.Key,
            Alias = schema.Alias,
            Properties = schema.Properties.Select(p => new PropertyInputSchemaResponseModel
            {
                Alias = p.Alias,
                DataTypeId = p.DataTypeKey,
                EditorAlias = p.EditorAlias,
                Mandatory = p.Mandatory,
                Variations = p.Variations.ToString(),
            }),
            IsElement = schema.IsElement,
            Variations = schema.Variations.ToString(),
        };
}
