namespace Umbraco.Cms.Core.Services.OperationStatus;

/// <summary>
///     Represents the status of an Umbraco upgrade operation.
/// </summary>
public enum UpgradeOperationStatus
{
    /// <summary>
    ///     The upgrade completed successfully.
    /// </summary>
    Success,

    /// <summary>
    ///     The upgrade operation failed.
    /// </summary>
    UpgradeFailed,
}
