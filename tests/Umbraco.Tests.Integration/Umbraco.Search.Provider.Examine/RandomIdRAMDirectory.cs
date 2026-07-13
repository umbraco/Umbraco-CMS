using Lucene.Net.Store;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Search.Provider.Examine;

public class RandomIdRAMDirectory : RAMDirectory
{
    private readonly string _lockId = Guid.NewGuid().ToString();

    public override string GetLockID() => _lockId;
}
