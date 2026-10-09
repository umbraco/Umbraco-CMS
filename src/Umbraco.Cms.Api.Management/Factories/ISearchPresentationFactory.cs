using Umbraco.Cms.Api.Management.ViewModels.Search.Facets;
using Umbraco.Cms.Api.Management.ViewModels.Search.Filters;
using Umbraco.Cms.Api.Management.ViewModels.Search.Sorters;
using Umbraco.Cms.Core.Search.Querying.Faceting;
using Umbraco.Cms.Core.Search.Querying.Filtering;
using Umbraco.Cms.Core.Search.Querying.Sorting;

namespace Umbraco.Cms.Api.Management.Factories;

/// <summary>
/// Converts between the Management API search presentation models and the search query and result types.
/// </summary>
public interface ISearchPresentationFactory
{
    /// <summary>
    /// Creates a search filter from its request model.
    /// </summary>
    /// <param name="filter">The filter request model.</param>
    /// <returns>The search filter.</returns>
    /// <exception cref="NotSupportedException">The request model type is not supported.</exception>
    Filter CreateFilter(IFilterRequestModel filter);

    /// <summary>
    /// Creates a search facet from its request model.
    /// </summary>
    /// <param name="facet">The facet request model.</param>
    /// <returns>The search facet.</returns>
    /// <exception cref="NotSupportedException">The request model type is not supported.</exception>
    Facet CreateFacet(IFacetRequestModel facet);

    /// <summary>
    /// Creates a search sorter from its request model.
    /// </summary>
    /// <param name="sorter">The sorter request model.</param>
    /// <returns>The search sorter.</returns>
    /// <exception cref="NotSupportedException">The request model type is not supported.</exception>
    Sorter CreateSorter(ISorterRequestModel sorter);

    /// <summary>
    /// Creates the response model for a facet result bucket.
    /// </summary>
    /// <param name="facetValue">The facet result bucket.</param>
    /// <returns>The facet value response model.</returns>
    /// <exception cref="NotSupportedException">The facet value type is not supported.</exception>
    IFacetValueResponseModel CreateFacetValueResponseModel(FacetValue facetValue);
}
