namespace Umbraco.Cms.Api.Management.ViewModels.LogViewer;

/// <summary>
/// Represents a presentation model for a property of a log message in the log viewer.
/// </summary>
public class LogMessagePropertyPresentationModel
{
    /// <summary>
    ///     Gets or sets the name of the log message property.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    ///     Gets or sets the value of the log message property (can be null).
    /// </summary>
    public string? Value { get; set; }
}
