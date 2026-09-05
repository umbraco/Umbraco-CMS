using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services;

/// <summary>
/// Provides helper services for dictionary-related operations, such as path calculation.
/// </summary>
public class DictionaryService : IDictionaryService
{
    private readonly IDictionaryItemService _dictionaryItemService;

    /// <summary>
    /// Initializes a new instance of the <see cref="DictionaryService"/> class.
    /// </summary>
    /// <param name="dictionaryItemService">The dictionary item service.</param>
    public DictionaryService(IDictionaryItemService dictionaryItemService) => _dictionaryItemService = dictionaryItemService;

    /// <inheritdoc />
    public async Task<string> CalculatePathAsync(Guid? parentId, int sourceId)
    {
        if (parentId.HasValue is false)
        {
            return "-1," + sourceId;
        }

        var ids = new List<int> { -1 };
        var parentIds = new List<int>();
        await GetParentIdAsync(parentId.Value, parentIds);
        parentIds.Reverse();
        ids.AddRange(parentIds);
        ids.Add(sourceId);
        return string.Join(",", ids);
    }

    private async Task GetParentIdAsync(Guid parentId, List<int> ids)
    {
        IDictionaryItem? dictionary = await _dictionaryItemService.GetAsync(parentId);
        if (dictionary == null)
        {
            return;
        }

        ids.Add(dictionary.Id);

        if (dictionary.ParentId.HasValue)
        {
            await GetParentIdAsync(dictionary.ParentId.Value, ids);
        }
    }
}
