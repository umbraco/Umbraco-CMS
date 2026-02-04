using System.Text.Json.Nodes;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;

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
    public async Task<Type?> GetValueTypeAsync(Guid dataTypeKey)
    {
        var dataType = await _dataTypeService.GetAsync(dataTypeKey);
        if (dataType is null)
        {
            return null;
        }

        return GetValueType(dataType.EditorAlias, dataType.ConfigurationObject);
    }

    /// <inheritdoc />
    public async Task<JsonObject?> GetValueSchemaAsync(Guid dataTypeKey)
    {
        var dataType = await _dataTypeService.GetAsync(dataTypeKey);
        if (dataType is null)
        {
            return null;
        }

        return GetValueSchema(dataType.EditorAlias, dataType.ConfigurationObject);
    }

    /// <inheritdoc />
    public Type? GetValueType(string propertyEditorAlias, object? configuration)
    {
        IValueSchemaProvider? provider = GetSchemaProvider(propertyEditorAlias);
        return provider?.GetValueType(configuration);
    }

    /// <inheritdoc />
    public JsonObject? GetValueSchema(string propertyEditorAlias, object? configuration)
    {
        IValueSchemaProvider? provider = GetSchemaProvider(propertyEditorAlias);
        return provider?.GetValueSchema(configuration);
    }

    /// <inheritdoc />
    public bool SupportsSchema(string propertyEditorAlias)
        => GetSchemaProvider(propertyEditorAlias) is not null;

    private IValueSchemaProvider? GetSchemaProvider(string propertyEditorAlias)
    {
        IDataEditor? editor = _dataEditors.FirstOrDefault(e => e.Alias == propertyEditorAlias);
        return editor as IValueSchemaProvider;
    }
}
