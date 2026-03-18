using System.Text.Json.Nodes;
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

    private static Type? GetValueTypeFromProvider(IValueSchemaProvider provider, object? configuration)
        => provider.GetValueType(configuration);

    private static JsonObject? GetValueSchemaFromProvider(IValueSchemaProvider provider, object? configuration)
        => provider.GetValueSchema(configuration);

    private IValueSchemaProvider? GetSchemaProvider(string propertyEditorAlias)
    {
        IDataEditor? editor = _dataEditors.FirstOrDefault(e => e.Alias == propertyEditorAlias);
        return editor as IValueSchemaProvider;
    }
}
