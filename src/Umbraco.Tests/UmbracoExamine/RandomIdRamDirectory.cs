using System;
using Lucene.Net.Store;

namespace Umbraco.Tests.UmbracoExamine
{
    /// <summary>
    /// This is needed for all tests, else the lockid collides with directories during testing
    /// </summary>
    public class RandomIdRamDirectory : RAMDirectory
    {
        private readonly string _lockId = Guid.NewGuid().ToString();
        public override string GetLockId()
        {
            return _lockId;
        }
    }
}