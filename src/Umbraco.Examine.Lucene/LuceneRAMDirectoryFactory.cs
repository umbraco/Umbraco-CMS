// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.IO;
using Examine.Lucene.Directories;
using Lucene.Net.Store;
using Directory = Lucene.Net.Store.Directory;

namespace Umbraco.Cms.Infrastructure.Examine
{
    public class LuceneRAMDirectoryFactory : IDirectoryFactory
    {
        public LuceneRAMDirectoryFactory()
        {
        }

        public Directory CreateDirectory(string indexName) => new RandomIdRAMDirectory();

        private class RandomIdRAMDirectory : RAMDirectory
        {
            private readonly string _lockId = Guid.NewGuid().ToString();
            public override string GetLockID() => _lockId;
        }
    }
}
