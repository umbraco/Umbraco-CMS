using Examine;

namespace Umbraco.Cms.Infrastructure.Examine;

/// <summary>
///
/// </summary>
public class IndexRebuildStatusManager : IIndexRebuildStatusManager
{
    IDictionary<string,bool> _rebuildingStatus = new Dictionary<string, bool>();
    public IndexRebuildStatusManager(IExamineManager examineManager)
    {
        foreach (var index in examineManager.Indexes)
        {
            _rebuildingStatus.Add(index.Name, false);
        }
    }

    public void SetRebuildingIndexStatus(IEnumerable<string> indexes, bool isRebuilding)
    {
        foreach (var index in indexes)
        {
            _rebuildingStatus[index] = isRebuilding;
        }
    }

    public bool GetRebuildingIndexStatus(string index) => _rebuildingStatus.TryGetValue(index, out var isRebuilding) && isRebuilding;
}
