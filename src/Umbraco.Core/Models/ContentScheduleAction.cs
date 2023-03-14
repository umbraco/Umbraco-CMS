namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Defines scheduled actions for documents.
/// </summary>
public enum ContentScheduleAction
{
    /// <summary>
    ///     Release the document.
    /// </summary>
    Release,

    /// <summary>
    ///     Expire the document.
    /// </summary>
    Expire,
}
