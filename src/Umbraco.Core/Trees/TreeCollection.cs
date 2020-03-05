using System.Collections.Generic;
using Umbraco.Core.Composing;

namespace Umbraco.Web.Trees
{
    /// <summary>
    /// Represents the collection of section trees.
    /// </summary>
    public class TreeCollection : BuilderCollectionBase<Tree>
    {
        public TreeCollection(IEnumerable<Tree> items)
            : base(items)
        { }
    }
}
