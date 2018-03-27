using System.Collections.Generic;
using Examine;

namespace Umbraco.Examine
{
    /// <summary>
    /// Returns a collection of <see cref="IIndexer"/>
    /// </summary>
    public interface IExamineIndexCollectionAccessor
    {
        IReadOnlyDictionary<string, IIndexer> Indexes { get; }
    }
}
