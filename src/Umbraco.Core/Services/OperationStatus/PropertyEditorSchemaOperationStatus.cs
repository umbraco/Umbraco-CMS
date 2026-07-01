namespace Umbraco.Cms.Core.Services.OperationStatus;

/// <summary>
/// Represents the status of a property editor schema operation.
/// </summary>
public enum PropertyEditorSchemaOperationStatus
{
    /// <summary>
    /// The operation completed successfully.
    /// </summary>
    Success,

    /// <summary>
    /// The specified data type was not found.
    /// </summary>
    DataTypeNotFound,

    /// <summary>
    /// The property editor does not support schema information (does not implement IValueSchemaProvider).
    /// </summary>
    SchemaNotSupported,
}
