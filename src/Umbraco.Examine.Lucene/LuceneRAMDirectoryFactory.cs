using Lucene.Net.Store;
using System;

namespace Umbraco.Examine
{
    public class LuceneRAMDirectoryFactory : ILuceneDirectoryFactory
    {
        public LuceneRAMDirectoryFactory()
        {

        }

        public Lucene.Net.Store.Directory CreateDirectory(string indexName) => new RandomIdRAMDirectory();

        private class RandomIdRAMDirectory : RAMDirectory
        {
            private readonly string _lockId = Guid.NewGuid().ToString();
            public override string GetLockId()
            {
                return _lockId;
            }
        }
    }
}
