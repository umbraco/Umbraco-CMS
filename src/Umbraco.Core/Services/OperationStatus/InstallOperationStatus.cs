namespace Umbraco.Cms.Core.Services.OperationStatus;

public enum InstallOperationStatus
{
    Success,
    UnknownDatabaseProvider,
    MissingConnectionString,
    MissingProviderName,
    DatabaseConnectionFailed,
    InstallFailed,
}
