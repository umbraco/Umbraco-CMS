namespace Umbraco.Cms.Api.Management.ViewModels.User;

public class LinkedLoginsRequestModel
{
    public IEnumerable<LinkedLoginViewModel> LinkedLogins { get; set; } = Enumerable.Empty<LinkedLoginViewModel>();
}
