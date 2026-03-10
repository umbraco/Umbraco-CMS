namespace Umbraco.Cms.Core.Services.Changes;

/// <summary>
///     Represents the types of changes that can occur on a domain.
/// </summary>
public enum DomainChangeTypes : byte
{
    /// <summary>
    ///     No change has occurred.
    /// </summary>
    None = 0,

    /// <summary>
    ///     All domains have been refreshed.
    /// </summary>
    RefreshAll = 1,

    /// <summary>
    ///     A single domain has been refreshed.
    /// </summary>
    Refresh = 2,

    /// <summary>
    ///     A domain has been removed.
    /// </summary>
    Remove = 3,
}
