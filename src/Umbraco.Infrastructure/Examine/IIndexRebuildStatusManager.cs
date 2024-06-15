using Examine;

namespace Umbraco.Cms.Infrastructure.Examine;

public interface IIndexRebuildStatusManager
{
    IndexStatus? GetRebuildingIndexStatus(string index);
    void SetReindexStatus(IIndex index, IEnumerable<IIndexPopulator> where);

    public void UpdatePopulatorStatus(string index, string populator, bool isRunning, int currentBatch, int totalBatches);
}

public class IndexStatus
{
    public bool IsRebuilding { get; set; }
    public IEnumerable<PopulatorStatus>? PopulatorStatuses { get; set; }
}

public class PopulatorStatus
{
    public PopulatorStatus(string name)
    {
        Name = name;
    }

    public string Name { get; set; }
    public bool IsRunning { get; set; }
    public int CurrentBatch { get; set; }
    public int TotalBatches { get; set; }
}
