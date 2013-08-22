using System;
using System.Collections.Generic;

namespace Umbraco.Core.Events
{
    public class RecycleBinEventArgs : CancellableEventArgs
    {
        public RecycleBinEventArgs(Guid nodeObjectType, IEnumerable<int> ids, List<string> files, bool emptiedSuccessfully)
            : base(false)
        {
            NodeObjectType = nodeObjectType;
            Ids = ids;
            Files = files;
            RecycleBinEmptiedSuccessfully = emptiedSuccessfully;
        }

        public RecycleBinEventArgs(Guid nodeObjectType, IEnumerable<int> ids, List<string> files)
            : base(true)
        {
            NodeObjectType = nodeObjectType;
            Ids = ids;
            Files = files;
        }

        /// <summary>
        /// Gets the Id of the node object type of the items 
        /// being deleted from the Recycle Bin.
        /// </summary>
        public Guid NodeObjectType { get; private set; }
        
        /// <summary>
        /// Gets the list of Ids for each of the items being deleted from the Recycle Bin.
        /// </summary>
        public IEnumerable<int> Ids { get; private set; }

        /// <summary>
        /// Gets the list of Files that should be deleted as part
        /// of emptying the Recycle Bin.
        /// </summary>
        public List<string> Files { get; private set; }

        /// <summary>
        /// Boolean indicating whether the Recycle Bin was emptied successfully
        /// </summary>
        public bool RecycleBinEmptiedSuccessfully { get; private set; }

        /// <summary>
        /// Boolean indicating whether this event was fired for the Content's Recycle Bin.
        /// </summary>
        public bool IsContentRecycleBin
        {
            get { return NodeObjectType == new Guid(Constants.ObjectTypes.Document); }
        }

        /// <summary>
        /// Boolean indicating whether this event was fired for the Media's Recycle Bin.
        /// </summary>
        public bool IsMediaRecycleBin
        {
            get { return NodeObjectType == new Guid(Constants.ObjectTypes.Media); }
        }
    }
}