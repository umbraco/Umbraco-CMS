using System.Collections.Generic;
using Examine;

namespace Umbraco.Examine
{
    public interface IIndexPopulator
    {
        /// <summary>
        /// If this index is registered with this populatr
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        bool IsRegistered(IIndex index);

        /// <summary>
        /// Populate indexers 
        /// </summary>
        /// <param name="indexes"></param>
        void Populate(params IIndex[] indexes);
    }
    
}
