using Umbraco.Cms.Api.Management.ViewModels.Document;

namespace Umbraco.Cms.Api.Management.ViewModels.Element;

public class PublishElementRequestModel
{
    public required IEnumerable<CultureAndScheduleRequestModel> PublishSchedules { get; set; }
}
