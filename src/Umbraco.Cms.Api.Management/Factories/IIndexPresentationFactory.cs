using Examine;
using Umbraco.Cms.Api.Management.ViewModels.Indexer;

namespace Umbraco.Cms.Api.Management.Factories;

public interface IIndexPresentationFactory
{
    [Obsolete("Use CreateAsync() instead. Scheduled for removal in Umbraco 19.")]
    IndexResponseModel Create(IIndex index);

    Task<IndexResponseModel> CreateAsync(IIndex index) => Task.FromResult(Create(index));
}
