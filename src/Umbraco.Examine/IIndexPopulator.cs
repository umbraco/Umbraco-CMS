using System.Collections.Generic;
using Examine;

namespace Umbraco.Examine
{
    public interface IIndexPopulator
    {
        /// <summary>
        /// Populate indexers 
        /// </summary>
        /// <param name="indexes"></param>
        void Populate(params IIndex[] indexes);
    }
    
}
