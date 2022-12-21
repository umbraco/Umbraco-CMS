using Umbraco.Cms.Api.Management.ViewModels.RedirectUrlManagement;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Api.Management.Factories;

public interface IRedirectUrlViewModelFactory
{
    RedirectUrlViewModel Create(IRedirectUrl source);

    IEnumerable<RedirectUrlViewModel> CreateMany(IEnumerable<IRedirectUrl> sources);
}
