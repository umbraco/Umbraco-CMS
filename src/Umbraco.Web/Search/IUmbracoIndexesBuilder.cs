using System.Collections.Generic;
using Examine;

namespace Umbraco.Web.Search
{
    internal interface IUmbracoIndexesBuilder
    {
        IEnumerable<IIndexer> Create();
    }
}
