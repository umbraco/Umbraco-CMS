using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Core.Services;

public interface ITreeEntitySortingService
{
    IEnumerable<TTreeEntity> SortEntities<TTreeEntity>(IEnumerable<TTreeEntity> entities, IEnumerable<SortingModel> sortingModels)
        where TTreeEntity : ITreeEntity;
}
