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

        if (results.Details is null || results.Details.Count == 0)
        {
            // No details, create a generic error from the top-level result
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
                errors.Add(new SchemaValidationResult("Value does not conform to schema"));
            }

            return errors;
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
            else
            {
                // Recursively check nested details
                errors.AddRange(ExtractValidationErrors(detail));
            }
        }

        return errors;
    }

    private IValueSchemaProvider? GetSchemaProvider(string propertyEditorAlias)
    {
        IDataEditor? editor = _dataEditors.FirstOrDefault(e => e.Alias == propertyEditorAlias);
        return editor as IValueSchemaProvider;
    }
}
