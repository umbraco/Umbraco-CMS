using System.Collections.Generic;
using Examine;

namespace Umbraco.Examine
{
    

    public interface IIndexPopulator
    {
        bool IsRegistered(string indexName);
        void RegisterIndex(string indexName);

        /// <summary>
        /// Populate indexers 
        /// </summary>
        /// <param name="indexes"></param>
        void Populate(params IIndex[] indexes);
    }
    
}
