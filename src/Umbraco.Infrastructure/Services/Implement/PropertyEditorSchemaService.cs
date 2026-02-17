using System.Text.Json;
using System.Text.Json.Nodes;
using Json.Schema;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Infrastructure.Services.Implement;

/// <summary>
/// Provides services for querying property editor value schemas.
/// </summary>
internal sealed class PropertyEditorSchemaService : IPropertyEditorSchemaService
{
    private readonly IDataTypeService _dataTypeService;
    private readonly DataEditorCollection _dataEditors;

    /// <summary>
    /// Initializes a new instance of the <see cref="PropertyEditorSchemaService"/> class.
    /// </summary>
    public PropertyEditorSchemaService(
        IDataTypeService dataTypeService,
        DataEditorCollection dataEditors)
    {
        _dataTypeService = dataTypeService;
        _dataEditors = dataEditors;
    }

    /// <inheritdoc />
    public async Task<Attempt<PropertyValueSchema, PropertyEditorSchemaOperationStatus>> GetSchemaAsync(Guid dataTypeKey)
    {
        var dataType = await _dataTypeService.GetAsync(dataTypeKey);
        if (dataType is null)
        {
            return Attempt.FailWithStatus(PropertyEditorSchemaOperationStatus.DataTypeNotFound, new PropertyValueSchema(null, null));
        }

        IValueSchemaProvider? provider = GetSchemaProvider(dataType.EditorAlias);
        if (provider is null)
        {
            return Attempt.FailWithStatus(PropertyEditorSchemaOperationStatus.SchemaNotSupported, new PropertyValueSchema(null, null));
        }

        Type? valueType = GetValueTypeFromProvider(provider, dataType.ConfigurationObject);
        JsonObject? jsonSchema = GetValueSchemaFromProvider(provider, dataType.ConfigurationObject);

        return Attempt.SucceedWithStatus(
            PropertyEditorSchemaOperationStatus.Success,
            new PropertyValueSchema(valueType, jsonSchema));
    }

    /// <inheritdoc />
    public Type? GetValueType(string propertyEditorAlias, object? configuration)
    {
        IValueSchemaProvider? provider = GetSchemaProvider(propertyEditorAlias);
        return provider is not null ? GetValueTypeFromProvider(provider, configuration) : null;
    }

    /// <inheritdoc />
    public JsonObject? GetValueSchema(string propertyEditorAlias, object? configuration)
    {
        IValueSchemaProvider? provider = GetSchemaProvider(propertyEditorAlias);
        return provider is not null ? GetValueSchemaFromProvider(provider, configuration) : null;
    }

    /// <inheritdoc />
    public bool SupportsSchema(string propertyEditorAlias)
        => GetSchemaProvider(propertyEditorAlias) is not null;

    /// <inheritdoc />
    public async Task<Attempt<IEnumerable<SchemaValidationResult>, PropertyEditorSchemaOperationStatus>> ValidateValueAsync(Guid dataTypeKey, object? value)
    {
        Attempt<PropertyValueSchema, PropertyEditorSchemaOperationStatus> schemaAttempt = await GetSchemaAsync(dataTypeKey);
        if (schemaAttempt.Success is false)
        {
            return Attempt.FailWithStatus(schemaAttempt.Status, Enumerable.Empty<SchemaValidationResult>());
        }

        JsonObject? schemaJson = schemaAttempt.Result.JsonSchema;
        if (schemaJson is null)
        {
            // Schema provider returned null schema - validation passes
            return Attempt.SucceedWithStatus(PropertyEditorSchemaOperationStatus.Success, Enumerable.Empty<SchemaValidationResult>());
        }

        try
        {
            // Parse the schema
            JsonSchema schema = JsonSchema.FromText(schemaJson.ToJsonString());

            // Convert value to JsonNode for evaluation
            JsonNode? valueNode = ConvertToJsonNode(value);

            // Evaluate the value against the schema
            EvaluationOptions options = new()
            {
                OutputFormat = OutputFormat.List,
            };

            EvaluationResults results = schema.Evaluate(valueNode, options);

            if (results.IsValid)
            {
                return Attempt.SucceedWithStatus(PropertyEditorSchemaOperationStatus.Success, Enumerable.Empty<SchemaValidationResult>());
            }

            // Collect validation errors
            return Attempt.SucceedWithStatus(PropertyEditorSchemaOperationStatus.Success, ExtractValidationErrors(results).AsEnumerable());
        }
        catch (JsonException ex)
        {
            return Attempt.SucceedWithStatus(PropertyEditorSchemaOperationStatus.Success, new[] { new SchemaValidationResult($"Invalid JSON: {ex.Message}") }.AsEnumerable());
        }
    }

    private static Type? GetValueTypeFromProvider(IValueSchemaProvider provider, object? configuration)
        => provider.GetValueType(configuration);

    private static JsonObject? GetValueSchemaFromProvider(IValueSchemaProvider provider, object? configuration)
        => provider.GetValueSchema(configuration);

    private static JsonNode? ConvertToJsonNode(object? value)
    {
        if (value is null)
        {
            return null;
        }

        if (value is JsonNode node)
        {
            return node;
        }

        if (value is JsonElement element)
        {
            return element.Deserialize<JsonNode?>();
        }

        if (value is string stringValue)
        {
            // Try to parse as JSON, otherwise treat as string literal
            try
            {
                return JsonNode.Parse(stringValue);
            }
            catch (JsonException)
            {
                return JsonValue.Create(stringValue);
            }
        }

        // Serialize other objects to JSON and parse
        var json = JsonSerializer.Serialize(value);
        return JsonNode.Parse(json);
    }

    private static List<SchemaValidationResult> ExtractValidationErrors(EvaluationResults results)
    {
        var errors = new List<SchemaValidationResult>();
        CollectValidationErrors(results, errors);

        // JSON Schema validators report the same logical error multiple times through different
        // schema evaluation paths. Deduplicate while collecting paths in a single pass.
        var seen = new HashSet<(string, string?, string?)>();
        var allPaths = new HashSet<string>();
        errors.RemoveAll(e =>
        {
            var isDuplicate = !seen.Add((e.Message, e.Path, e.Keyword));
            if (!isDuplicate && e.Path is not null)
            {
                allPaths.Add(e.Path);
            }

            return isDuplicate;
        });

        // When all errors lack paths, there's nothing to filter
        if (allPaths.Count == 0)
        {
            return errors;
        }

        // Schema validators bubble up failures to parent paths with generic "does not conform"
        // messages. These add noise when we already have the specific child error. Identify
        // which paths are parents of other paths so we can filter out their generic errors.
        var parentPathsWithChildren = new HashSet<string>();
        foreach (var path in allPaths)
        {
            var lastSlash = path.LastIndexOf('/');
            while (lastSlash > 0)
            {
                var parentPath = path[..lastSlash];
                if (allPaths.Contains(parentPath))
                {
                    parentPathsWithChildren.Add(parentPath);
                }

                lastSlash = parentPath.LastIndexOf('/');
            }
        }

        // Determine if there are errors at deeper levels (non-root paths). Root path is "" in JSON Pointer.
        var hasDeepErrors = allPaths.Any(p => p.Length > 0);

        // Only filter pathless errors (Path is null) when more specific path errors exist.
        // Don't filter empty paths ("") as this represents the root location which is valid.
        // Similarly, filter generic parent errors (keyword=null) only when specific child errors exist.
        errors.RemoveAll(e =>
            (e.Path is null && hasDeepErrors) ||
            (e.Keyword is null && parentPathsWithChildren.Contains(e.Path ?? string.Empty)));

        return errors;
    }

    private static void CollectValidationErrors(EvaluationResults results, List<SchemaValidationResult> errors)
    {
        if (results.Details is null || results.Details.Count == 0)
        {
            // No details, create an error from the current result
            if (!results.IsValid && results.Errors is not null)
            {
                foreach (var error in results.Errors)
                {
                    errors.Add(new SchemaValidationResult(
                        error.Value,
                        results.InstanceLocation?.ToString(),
                        error.Key));
                }
            }
            else if (!results.IsValid)
            {
                errors.Add(new SchemaValidationResult(
                    "Value does not conform to schema",
                    results.InstanceLocation?.ToString()));
            }

            return;
        }

        // Process nested results
        foreach (EvaluationResults detail in results.Details)
        {
            if (detail.IsValid)
            {
                continue;
            }

            if (detail.Errors is not null && detail.Errors.Count > 0)
            {
                foreach (var error in detail.Errors)
                {
                    errors.Add(new SchemaValidationResult(
                        error.Value,
                        detail.InstanceLocation?.ToString(),
                        error.Key));
                }
            }

            // Always recurse to find more specific errors
            CollectValidationErrors(detail, errors);
        }
    }

    private IValueSchemaProvider? GetSchemaProvider(string propertyEditorAlias)
    {
        IDataEditor? editor = _dataEditors.FirstOrDefault(e => e.Alias == propertyEditorAlias);
        return editor as IValueSchemaProvider;
    }
}
