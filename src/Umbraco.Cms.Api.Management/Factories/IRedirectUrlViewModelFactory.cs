using Umbraco.Cms.Api.Management.ViewModels.RedirectUrlManagement;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Api.Management.Factories;

public interface IRedirectUrlViewModelFactory
{
    RedirectUrlResponseModel Create(IRedirectUrl source);

    IEnumerable<RedirectUrlResponseModel> CreateMany(IEnumerable<IRedirectUrl> sources);
}
