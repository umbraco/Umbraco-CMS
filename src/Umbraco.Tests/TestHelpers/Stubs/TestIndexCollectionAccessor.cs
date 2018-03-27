using System.Collections.Generic;
using Examine;
using Umbraco.Examine;

namespace Umbraco.Tests.TestHelpers.Stubs
{
    public class TestIndexCollectionAccessor : IExamineIndexCollectionAccessor
    {
        public IReadOnlyDictionary<string, IIndexer> Indexes => new Dictionary<string, IIndexer>();
    }
}
