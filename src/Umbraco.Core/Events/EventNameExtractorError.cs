namespace Umbraco.Cms.Core.Events;

/// <summary>
///     Represents errors that can occur during event name extraction.
/// </summary>
public enum EventNameExtractorError
{
    /// <summary>
    ///     No matching event was found.
    /// </summary>
    NoneFound,

    /// <summary>
    ///     Multiple matching events were found, making the result ambiguous.
    /// </summary>
    Ambiguous,
}
