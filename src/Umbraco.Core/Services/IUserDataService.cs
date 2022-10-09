using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services;

public interface IUserDataService
{
    IEnumerable<UserData> GetUserData();
}
