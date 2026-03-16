namespace Umbraco.Cms.Api.Management.ViewModels.LogViewer;

/// <summary>
/// Represents the response model for a log template in the log viewer API.
/// </summary>
public class LogTemplateResponseModel
{
    /// <summary>
    ///     Gets or sets the message template.
    /// </summary>
    public string? MessageTemplate { get; set; }

    /// <summary>
    ///     Gets or sets the count.
    /// </summary>
    public int Count { get; set; }
}
