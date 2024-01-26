using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services;

[Obsolete($"Use {nameof(ISystemTroubleshootingInformationService)} instead. Will be removed in V16.")]
public interface IUserDataService
{
    [Obsolete($"Use {nameof(ISystemTroubleshootingInformationService)} instead. Will be removed in V16.")]
    IEnumerable<UserData> GetUserData();
}
