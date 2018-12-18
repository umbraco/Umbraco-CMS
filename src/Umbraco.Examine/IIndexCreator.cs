using System.Collections.Generic;
using Examine;

namespace Umbraco.Examine
{
    /// <summary>
    /// Creates <see cref="IIndex"/>'s 
    /// </summary>
    public interface IIndexCreator
    {
        IEnumerable<IIndex> Create();
    }
}
