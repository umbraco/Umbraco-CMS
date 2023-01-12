using Examine;
using Umbraco.Cms.Api.Management.ViewModels.Search;

namespace Umbraco.Cms.Api.Management.Factories;

public interface IIndexViewModelFactory
{
    IndexViewModel Create(IIndex index);
}
