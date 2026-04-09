namespace Umbraco.Cms.Core.Events;

/// <summary>
///     The type of event message
/// </summary>
public enum EventMessageType
{
    /// <summary>
    ///     The default message type.
    /// </summary>
    Default = 0,

    /// <summary>
    ///     An informational message.
    /// </summary>
    Info = 1,

    /// <summary>
    ///     An error message.
    /// </summary>
    Error = 2,

    /// <summary>
    ///     A success message.
    /// </summary>
    Success = 3,

    /// <summary>
    ///     A warning message.
    /// </summary>
    Warning = 4,
}
