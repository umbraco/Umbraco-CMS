using System.Collections.Generic;
using Examine;

namespace Umbraco.Examine
{
    /// <summary>
    /// Default implementation of IExamineIndexCollectionAccessor to return indexes from Examinemanager
    /// </summary>
    public class ExamineIndexCollectionAccessor : IExamineIndexCollectionAccessor
    {
        public IReadOnlyDictionary<string, IIndexer> Indexes => ExamineManager.Instance.IndexProviders;
    }
}
