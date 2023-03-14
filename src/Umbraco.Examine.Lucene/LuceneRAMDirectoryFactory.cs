// Copyright (c) Umbraco.
// See LICENSE for more details.

using Examine.Lucene.Directories;
using Examine.Lucene.Providers;
using Lucene.Net.Store;
using Directory = Lucene.Net.Store.Directory;

namespace Umbraco.Cms.Infrastructure.Examine;

public class LuceneRAMDirectoryFactory : DirectoryFactoryBase
{
    protected override Directory CreateDirectory(LuceneIndex luceneIndex, bool forceUnlock)
        => new RandomIdRAMDirectory();

    private class RandomIdRAMDirectory : RAMDirectory
    {
        private readonly string _lockId = Guid.NewGuid().ToString();
        public override string GetLockID() => _lockId;
    }
}
