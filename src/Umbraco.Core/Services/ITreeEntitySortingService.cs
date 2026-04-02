using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Core.Services;

/// <summary>
/// Provides functionality for sorting tree entities based on sorting models.
/// </summary>
public interface ITreeEntitySortingService
{
    /// <summary>
    /// Sorts a collection of tree entities according to the specified sorting models.
    /// </summary>
    /// <typeparam name="TTreeEntity">The type of tree entity being sorted.</typeparam>
    /// <param name="entities">The collection of entities to sort.</param>
    /// <param name="sortingModels">The sorting models defining the desired sort order.</param>
    /// <returns>The sorted collection of entities.</returns>
    IEnumerable<TTreeEntity> SortEntities<TTreeEntity>(IEnumerable<TTreeEntity> entities, IEnumerable<SortingModel> sortingModels)
        where TTreeEntity : ITreeEntity;
}
