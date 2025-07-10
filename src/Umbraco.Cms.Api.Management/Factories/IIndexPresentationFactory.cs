using Examine;
using Umbraco.Cms.Api.Management.ViewModels.Indexer;

namespace Umbraco.Cms.Api.Management.Factories;

public interface IIndexPresentationFactory
{
    [Obsolete("Use the non-obsolete method instead. Scheduled for removal in v18.")]
    IndexResponseModel Create(IIndex index);

    Task<IndexResponseModel> CreateAsync(IIndex index) => Task.FromResult(Create(index));
}
