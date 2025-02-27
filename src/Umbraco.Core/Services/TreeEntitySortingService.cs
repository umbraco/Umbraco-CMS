using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Core.Services;

public class TreeEntitySortingService : ITreeEntitySortingService
{
    public IEnumerable<TTreeEntity> SortEntities<TTreeEntity>(IEnumerable<TTreeEntity> entities, IEnumerable<SortingModel> sortingModels)
        where TTreeEntity : ITreeEntity
    {
        TTreeEntity[] entitiesArray = entities as TTreeEntity[] ?? entities.ToArray();
        if (entitiesArray.DistinctBy(entity => entity.ParentId).Count() != 1)
        {
            throw new ArgumentException("Cannot sort entities under different parents", nameof(entities));
        }

        var sortedEntityKeys = entitiesArray.OrderBy(entity => entity.SortOrder).Select(entity => entity.Key).ToList();

        SortingModel[] sortingModelsArray = sortingModels as SortingModel[] ?? sortingModels.ToArray();

        if (sortingModelsArray.Any(sort => sort.SortOrder < 0 || sort.SortOrder >= sortedEntityKeys.Count))
        {
            throw new ArgumentException("Sort order index out of range - must be within the boundaries of the entities collection", nameof(sortingModels));
        }

        if (sortingModelsArray.Any(sort => sortedEntityKeys.Contains(sort.Key) is false))
        {
            throw new ArgumentException("One or more sort keys was not found in the entities collection", nameof(sortingModels));
        }

        if (sortingModelsArray.DistinctBy(sort => sort.Key).Count() != sortingModelsArray.Length)
        {
            throw new ArgumentException("One or more duplicate sort keys was found in the sorting collection", nameof(sortingModels));
        }

        if (sortingModelsArray.DistinctBy(sort => sort.SortOrder).Count() != sortingModelsArray.Length)
        {
            throw new ArgumentException("One or more duplicate sort orders was found in the sorting collection", nameof(sortingModels));
        }

        Guid[] keysBeingSorted = sortingModelsArray.Select(sort => sort.Key).ToArray();
        sortedEntityKeys.RemoveAll(keysBeingSorted.Contains);

        foreach (SortingModel sort in sortingModelsArray.OrderBy(sort => sort.SortOrder))
        {
            if (sort.SortOrder < sortedEntityKeys.Count)
            {
                sortedEntityKeys.Insert(sort.SortOrder, sort.Key);
            }
            else
            {
                sortedEntityKeys.Add(sort.Key);
            }
        }

        return sortedEntityKeys.Select(key => entitiesArray.Single(i => i.Key == key)).ToArray();
    }
}
