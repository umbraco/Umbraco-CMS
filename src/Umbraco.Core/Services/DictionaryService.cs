using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services;

/// <summary>
/// Provides helper services for dictionary-related operations, such as path calculation.
/// </summary>
public class DictionaryService : IDictionaryService
{
    private readonly ILocalizationService _localizationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="DictionaryService"/> class.
    /// </summary>
    /// <param name="localizationService">The localization service.</param>
    public DictionaryService(ILocalizationService localizationService) => _localizationService = localizationService;

    /// <inheritdoc />
    public string CalculatePath(Guid? parentId, int sourceId)
    {
        string path;

        // TODO: check if there is a better way
        if (parentId.HasValue)
        {
            var ids = new List<int> { -1 };
            var parentIds = new List<int>();
            GetParentId(parentId.Value, parentIds);
            parentIds.Reverse();
            ids.AddRange(parentIds);
            ids.Add(sourceId);
            path = string.Join(",", ids);
        }
        else
        {
            path = "-1," + sourceId;
        }

        return path;
    }

    private void GetParentId(Guid parentId, List<int> ids)
    {
        IDictionaryItem? dictionary = _localizationService.GetDictionaryItemById(parentId);
        if (dictionary == null)
        {
            return;
        }

        ids.Add(dictionary.Id);

        if (dictionary.ParentId.HasValue)
        {
            GetParentId(dictionary.ParentId.Value, ids);
        }
    }
}
