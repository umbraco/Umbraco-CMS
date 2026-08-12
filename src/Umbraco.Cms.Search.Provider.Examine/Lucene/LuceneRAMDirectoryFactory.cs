// Copyright (c) Umbraco.
// See LICENSE for more details.

using Examine.Lucene.Directories;
using Examine.Lucene.Providers;
using Lucene.Net.Store;
using Directory = Lucene.Net.Store.Directory;

namespace Umbraco.Cms.Search.Provider.Examine.Lucene;

/// <summary>
/// An Examine directory factory that creates an in-memory (non-persisted) Lucene directory, for tests/development.
/// </summary>
public class LuceneRAMDirectoryFactory : DirectoryFactoryBase
{
    /// <inheritdoc />
    protected override Directory CreateDirectory(LuceneIndex luceneIndex, bool forceUnlock)
        => new RandomIdRAMDirectory();

    private sealed class RandomIdRAMDirectory : RAMDirectory
    {
        private readonly string _lockId = Guid.NewGuid().ToString();
        public override string GetLockID() => _lockId;
    }
}
