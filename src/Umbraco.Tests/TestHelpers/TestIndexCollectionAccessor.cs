using System.Collections.Generic;
using Examine;
using UmbracoExamine;

namespace Umbraco.Tests.TestHelpers
{
    /// <summary>
    /// An empty index collection accessor class for testing
    /// </summary>
    public class TestIndexCollectionAccessor : IExamineIndexCollectionAccessor
    {
        public IReadOnlyDictionary<string, IExamineIndexer> Indexes
        {
            get { return new Dictionary<string, IExamineIndexer>(); }
        }
    }
}