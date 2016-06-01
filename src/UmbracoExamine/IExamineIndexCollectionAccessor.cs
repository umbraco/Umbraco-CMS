using System.Collections.Generic;
using Examine;

namespace UmbracoExamine
{
    /// <summary>
    /// Returns a collection of IExamineIndexer
    /// </summary>
    public interface IExamineIndexCollectionAccessor
    {
        IReadOnlyDictionary<string, IExamineIndexer> Indexes { get; }
    }
}