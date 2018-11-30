using System.Collections.Generic;
using Examine;

namespace Umbraco.Web.Search
{
    /// <summary>
    /// Used to create the Umbraco indexes
    /// </summary>
    public interface IUmbracoIndexesCreator
    {
        IEnumerable<IIndex> Create();
    }
}
