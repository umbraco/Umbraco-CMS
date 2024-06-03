namespace Umbraco.Cms.Infrastructure.Examine;

public interface IIndexRebuildStatusManager
{
    void SetRebuildingIndexStatus(IEnumerable<string> indexes, bool b);
    bool GetRebuildingIndexStatus(string index);
}
