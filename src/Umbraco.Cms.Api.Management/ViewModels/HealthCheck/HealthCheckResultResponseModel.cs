using Umbraco.Cms.Core.HealthChecks;

namespace Umbraco.Cms.Api.Management.ViewModels.HealthCheck;

public class HealthCheckResultResponseModel
{
    /// <summary>
    ///     Gets or sets the status message.
    /// </summary>
    public required string Message { get; set; }

    /// <summary>
    ///     Gets or sets the status type.
    /// </summary>
    public StatusResultType ResultType { get; set; }

    /// <summary>
    ///     Gets or sets the potential actions to take (if any).
    /// </summary>
    public IEnumerable<HealthCheckActionRequestModel>? Actions { get; set; }

    /// <summary>
    ///     This is optional but would allow a developer to get or set a link that is shown as a "Read more" button.
    /// </summary>
    public string? ReadMoreLink { get; set; }
}
