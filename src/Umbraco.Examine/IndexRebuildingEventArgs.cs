using System;
using System.Collections.Generic;
using Examine;

namespace Umbraco.Examine
{
    public class IndexRebuildingEventArgs : EventArgs
    {
        public IndexRebuildingEventArgs(IEnumerable<IIndex> indexes)
        {
            Indexes = indexes;
        }

        /// <summary>
        /// The indexes being rebuilt
        /// </summary>
        public IEnumerable<IIndex> Indexes { get; }
    }
}
