using Umbraco.Cms.Api.Management.ViewModels.Content;

namespace Umbraco.Cms.Api.Management.ViewModels.Document;

/// <summary>
/// Represents the data required to request the publishing of a document in the API.
/// </summary>
public class PublishDocumentRequestModel
{
    /// <summary>
    /// Gets or sets the collection of publish schedules for different cultures.
    /// </summary>
    public required IEnumerable<CultureAndScheduleRequestModel> PublishSchedules { get; set; }
}
