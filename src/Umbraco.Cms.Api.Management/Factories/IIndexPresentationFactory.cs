using Examine;
using Umbraco.Cms.Api.Management.ViewModels.Indexer;

namespace Umbraco.Cms.Api.Management.Factories;

public interface IIndexPresentationFactory
{
    IndexResponseModel Create(IIndex index);
}
