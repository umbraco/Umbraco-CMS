using Examine;
using Umbraco.Cms.Api.Management.ViewModels.Indexer;

namespace Umbraco.Cms.Api.Management.Factories;

public interface IIndexViewModelFactory
{
    IndexViewModel Create(IIndex index);
}
