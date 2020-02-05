using Lucene.Net.Store;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Umbraco.Tests.TestHelpers
{

    /// <summary>
    /// Used for tests with Lucene so that each RAM directory is unique
    /// </summary>
    public class RandomIdRAMDirectory : RAMDirectory
    {
        private readonly string _lockId = Guid.NewGuid().ToString();
        public override string GetLockId()
        {
            return _lockId;
        }
    }
}
