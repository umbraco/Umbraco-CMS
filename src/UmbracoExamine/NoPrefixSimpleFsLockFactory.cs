using System.IO;
using Examine;
using Examine.LuceneEngine;
using Lucene.Net.Store;

namespace UmbracoExamine
{
    /// <summary>
    /// A custom <see cref="SimpleFSLockFactory"/> that ensures a prefixless lock prefix
    /// </summary>
    /// <remarks>
    /// This is a work around for the Lucene APIs. By default Lucene will use a null prefix however when we set a custom
    /// lock factory the null prefix is overwritten.
    /// </remarks>
    public class NoPrefixSimpleFsLockFactory : SimpleFSLockFactory
    {
        public NoPrefixSimpleFsLockFactory(DirectoryInfo lockDir) : base(lockDir)
        {
        }

        public override void SetLockPrefix(string lockPrefix)
        {
            base.SetLockPrefix(null);
        }
    }
}