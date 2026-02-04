namespace Umbraco.Cms.Core.Services.OperationStatus;

/// <summary>
///     Represents the status of a temporary file XML import operation.
/// </summary>
public enum TemporaryFileXmlImportOperationStatus
{
    /// <summary>
    ///     The operation completed successfully.
    /// </summary>
    Success = 0,

    /// <summary>
    ///     The operation failed because the temporary file could not be found.
    /// </summary>
    TemporaryFileNotFound,

    /// <summary>
    ///     The operation failed because the entity type could not be determined from the XML content.
    /// </summary>
    UndeterminedEntityType,
}
