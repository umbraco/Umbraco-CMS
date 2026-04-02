namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Defines the status of whether something is up to date.
/// </summary>
public enum OutOfDateType
{
    /// <summary>
    ///     Indicates the item is out of date and needs updating.
    /// </summary>
    OutOfDate,

    /// <summary>
    ///     Indicates the item is current and up to date.
    /// </summary>
    Current,

    /// <summary>
    ///     Indicates the status could not be determined.
    /// </summary>
    Unknown = 100
}
