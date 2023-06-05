namespace Umbraco.Cms.Api.Management.ViewModels.User.Current;

public class UserDataResponseModel
{
    public IEnumerable<UserDataViewModel> UserData { get; set; } = Enumerable.Empty<UserDataViewModel>();
}
