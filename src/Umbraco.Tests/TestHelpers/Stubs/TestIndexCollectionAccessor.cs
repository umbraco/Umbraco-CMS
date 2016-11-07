using System.Collections.Generic;
using Examine;
using UmbracoExamine;

namespace Umbraco.Tests.TestHelpers.Stubs
{
    public class TestIndexCollectionAccessor : IExamineIndexCollectionAccessor
    {
        public IReadOnlyDictionary<string, IExamineIndexer> Indexes => new Dictionary<string, IExamineIndexer>();
    }
}