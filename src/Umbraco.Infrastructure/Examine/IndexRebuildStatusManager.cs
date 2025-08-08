using Examine;

namespace Umbraco.Cms.Infrastructure.Examine;

/// <summary>
///
/// </summary>
public class IndexRebuildStatusManager : IIndexRebuildStatusManager
{
    private readonly IExamineManager _examineManager;
    IDictionary<string, IndexStatus> _rebuildingStatus = new Dictionary<string, IndexStatus>();

    public IndexRebuildStatusManager(IExamineManager examineManager)
    {
        _examineManager = examineManager;
        foreach (var index in examineManager.Indexes)
        {
            _rebuildingStatus.Add(index.Name, new IndexStatus());
        }
    }

    public IndexStatus? GetRebuildingIndexStatus(string index) =>
        _rebuildingStatus.TryGetValue(index, out var status) ? status : null;

    public void SetReindexStatus(IIndex index, IEnumerable<IIndexPopulator> where)
    {
        if (!_rebuildingStatus.TryGetValue(index.Name, out var status))
        {
            status = new IndexStatus();
            _rebuildingStatus.Add(index.Name, status);
        }

        status.IsRebuilding = true;
        status.PopulatorStatuses = where.Select(x => new PopulatorStatus(x.GetType().Name));
    }

    public void UpdatePopulatorStatus(
        string index,
        string populator,
        bool isRunning,
        int currentBatch,
        int totalBatches)
    {
        var examineIndex = _examineManager.GetIndex(index);

        if (_rebuildingStatus.TryGetValue(index, out var status))
        {
            var populatorStatus = status?.PopulatorStatuses?.FirstOrDefault(x => x.Name == populator);
            if (populatorStatus != null)
            {
                populatorStatus.IsRunning = isRunning;
                populatorStatus.CurrentBatch = currentBatch;
                populatorStatus.TotalBatches = totalBatches;
            }

            examineIndex.IndexItems(
            [
                new ValueSet(
                    populator,
                    "populator",
                    new Dictionary<string, object>()
                    {
                        { "isRunning", isRunning },
                        { "currentBatch", currentBatch },
                        { "totalBatches", totalBatches }
                    })
            ]);
            return;
        }
        var currentStatus= new IndexStatus();
        _rebuildingStatus.Add(index,currentStatus);
        var newPopulatorStatus = new PopulatorStatus(populator)
        {
            IsRunning = isRunning,
            CurrentBatch = currentBatch,
            TotalBatches = totalBatches
        };
        currentStatus.PopulatorStatuses = new List<PopulatorStatus> { newPopulatorStatus };
        examineIndex.IndexItems(
            [
                new ValueSet(
                    populator,
                    "populator",
                    new Dictionary<string, object>()
                    {
                        { "isRunning", isRunning },
                        { "currentBatch", currentBatch },
                        { "totalBatches", totalBatches }
                    })
            ]);
    }
}
