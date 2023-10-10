using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Management.ViewModels.User.Current;

public class UserDataResponseModel : IResponseModel
{
    public IEnumerable<UserDataViewModel> UserData { get; set; } = Enumerable.Empty<UserDataViewModel>();
    public string Type => UdiEntityType.UserData;
}
