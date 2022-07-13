using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models.ContentEditing;

[DataContract]
public enum NotificationStyle
{
    /// <summary>
    ///     Save icon
    /// </summary>
    Save = 0,

    /// <summary>
    ///     Info icon
    /// </summary>
    Info = 1,

    /// <summary>
    ///     Error icon
    /// </summary>
    Error = 2,

    /// <summary>
    ///     Success icon
    /// </summary>
    Success = 3,

    /// <summary>
    ///     Warning icon
    /// </summary>
    Warning = 4,
}
