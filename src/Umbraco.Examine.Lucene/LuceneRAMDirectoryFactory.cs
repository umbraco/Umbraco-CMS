// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.IO;
using System.Threading;
using Examine.Lucene.Directories;
using Examine.Lucene.Providers;
using Lucene.Net.Store;
using Directory = Lucene.Net.Store.Directory;

namespace Umbraco.Cms.Infrastructure.Examine
{
    public class LuceneRAMDirectoryFactory : IDirectoryFactory
    {
        private Directory _directory;
        private bool _disposedValue;

        public LuceneRAMDirectoryFactory()
        {
        }

        public Directory CreateDirectory(LuceneIndex luceneIndex, bool forceUnlock)
            => LazyInitializer.EnsureInitialized(ref _directory, () => new RandomIdRAMDirectory());

        private class RandomIdRAMDirectory : RAMDirectory
        {
            private readonly string _lockId = Guid.NewGuid().ToString();
            public override string GetLockID() => _lockId;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _directory?.Dispose();
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
        }
    }
}
