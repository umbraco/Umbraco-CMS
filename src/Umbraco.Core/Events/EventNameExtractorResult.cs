namespace Umbraco.Cms.Core.Events;

/// <summary>
///     Represents the result of an event name extraction operation.
/// </summary>
public class EventNameExtractorResult
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="EventNameExtractorResult" /> class with a name.
    /// </summary>
    /// <param name="name">The extracted event name.</param>
    public EventNameExtractorResult(string? name) => Name = name;

    /// <summary>
    ///     Initializes a new instance of the <see cref="EventNameExtractorResult" /> class with an error.
    /// </summary>
    /// <param name="error">The error that occurred during extraction.</param>
    public EventNameExtractorResult(EventNameExtractorError? error) => Error = error;

    /// <summary>
    ///     Gets the error that occurred during extraction, if any.
    /// </summary>
    public EventNameExtractorError? Error { get; }

    /// <summary>
    ///     Gets the extracted event name, if successful.
    /// </summary>
    public string? Name { get; }
}
