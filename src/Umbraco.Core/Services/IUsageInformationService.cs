using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services;

public interface IUsageInformationService
{
    IEnumerable<UsageInformation>? GetDetailed();
}
