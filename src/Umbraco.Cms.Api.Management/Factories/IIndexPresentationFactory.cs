using Examine;
using Umbraco.Cms.Api.Management.ViewModels.Indexer;

namespace Umbraco.Cms.Api.Management.Factories;

/// <summary>
/// Defines a factory for creating index presentation models used in the Umbraco management API.
/// Implementations of this interface are responsible for constructing models that represent index views.
/// </summary>
public interface IIndexPresentationFactory
{
    /// <summary>
    /// Creates an <see cref="IndexResponseModel"/> from the given <see cref="IIndex"/>.
    /// </summary>
    /// <param name="index">The index to create the response model from.</param>
    /// <returns>The created <see cref="IndexResponseModel"/>.</returns>
    [Obsolete("Use CreateAsync() instead. Scheduled for removal in Umbraco 19.")]
    IndexResponseModel Create(IIndex index);

    /// <summary>
    /// Creates an <see cref="IndexResponseModel"/> asynchronously from the given <see cref="IIndex"/>.
    /// </summary>
    /// <param name="index">The index to create the response model from.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the created <see cref="IndexResponseModel"/>.</returns>
    Task<IndexResponseModel> CreateAsync(IIndex index) => Task.FromResult(Create(index));
}
