using System.Collections.Generic;
using Examine;

namespace Umbraco.Examine
{
    /// <summary>
    /// Returns a collection of IExamineIndexer
    /// </summary>
    public interface IExamineIndexCollectionAccessor
    {
        IReadOnlyDictionary<string, IExamineIndexer> Indexes { get; }
    }
}
