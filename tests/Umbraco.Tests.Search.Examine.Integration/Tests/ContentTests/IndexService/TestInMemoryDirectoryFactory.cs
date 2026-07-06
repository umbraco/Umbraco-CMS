using Examine.Lucene.Directories;
using Examine.Lucene.Providers;
using Directory = Lucene.Net.Store.Directory;

namespace Umbraco.Tests.Search.Examine.Integration.Tests.ContentTests.IndexService;

public class TestInMemoryDirectoryFactory : DirectoryFactoryBase
{
    private RandomIdRAMDirectory _randomIdRamDirectory = null!;

    protected override Directory CreateDirectory(LuceneIndex luceneIndex, bool forceUnlock)
    {
        _randomIdRamDirectory = new RandomIdRAMDirectory();
        return _randomIdRamDirectory;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _randomIdRamDirectory.Dispose();
    }
}
