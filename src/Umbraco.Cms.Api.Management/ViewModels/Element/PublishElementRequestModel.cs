using Umbraco.Cms.Api.Management.ViewModels.Document;

namespace Umbraco.Cms.Api.Management.ViewModels.Element;

/// <summary>
/// Represents the data required to request the publishing of an element in the API.
/// </summary>
public class PublishElementRequestModel
{
    /// <summary>
    /// Gets or sets the collection of publish schedules for different cultures.
    /// </summary>
    public required IEnumerable<CultureAndScheduleRequestModel> PublishSchedules { get; set; }
}
