using System;
using System.Collections.Generic;

namespace Umbraco.Core.Events
{
    public class RecycleBinEventArgs : CancellableEventArgs
    {
        public RecycleBinEventArgs(Guid nodeObjectType, IEnumerable<string> files)
            : base(false)
        {
            NodeObjectType = nodeObjectType;
            Files = files;
        }

        /// <summary>
        /// Gets the Id of the node object type of the items 
        /// being deleted from the Recycle Bin.
        /// </summary>
        public Guid NodeObjectType { get; private set; }

        /// <summary>
        /// Gets the list of Files that should be deleted as part
        /// of emptying the Recycle Bin.
        /// </summary>
        public IEnumerable<string> Files { get; private set; }
    }
}