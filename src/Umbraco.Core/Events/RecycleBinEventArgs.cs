using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models;

namespace Umbraco.Core.Events
{
    public class RecycleBinEventArgs : CancellableEventArgs
    {
        public RecycleBinEventArgs(Guid nodeObjectType, Dictionary<int, IEnumerable<Property>> allPropertyData, bool emptiedSuccessfully)
            : base(false)
        {
            AllPropertyData = allPropertyData;
            NodeObjectType = nodeObjectType;
            Ids = AllPropertyData.Select(x => x.Key);            
            RecycleBinEmptiedSuccessfully = emptiedSuccessfully;
            Files = new List<string>();
        }

        public RecycleBinEventArgs(Guid nodeObjectType, Dictionary<int, IEnumerable<Property>> allPropertyData)
            : base(true)
        {
            AllPropertyData = allPropertyData;
            NodeObjectType = nodeObjectType;
            Ids = AllPropertyData.Select(x => x.Key);
            Files = new List<string>();
        }

        /// <summary>
        /// Backwards compatibility constructor
        /// </summary>
        /// <param name="nodeObjectType"></param>
        /// <param name="allPropertyData"></param>
        /// <param name="files"></param>
        /// <param name="emptiedSuccessfully"></param>
        internal RecycleBinEventArgs(Guid nodeObjectType, Dictionary<int, IEnumerable<Property>> allPropertyData, List<string> files, bool emptiedSuccessfully)
            : base(false)
        {
            AllPropertyData = allPropertyData;
            NodeObjectType = nodeObjectType;
            Ids = AllPropertyData.Select(x => x.Key);
            RecycleBinEmptiedSuccessfully = emptiedSuccessfully;
            Files = files;
        }

        /// <summary>
        /// Backwards compatibility constructor
        /// </summary>
        /// <param name="nodeObjectType"></param>
        /// <param name="allPropertyData"></param>
        /// <param name="files"></param>
        internal RecycleBinEventArgs(Guid nodeObjectType, Dictionary<int, IEnumerable<Property>> allPropertyData, List<string> files)
            : base(true)
        {
            AllPropertyData = allPropertyData;
            NodeObjectType = nodeObjectType;
            Ids = AllPropertyData.Select(x => x.Key);
            Files = files;
        }

        [Obsolete("Use the other ctor that specifies all property data instead")]
        public RecycleBinEventArgs(Guid nodeObjectType, IEnumerable<int> ids, List<string> files, bool emptiedSuccessfully)
            : base(false)
        {
            NodeObjectType = nodeObjectType;
            Ids = ids;
            Files = files;
            RecycleBinEmptiedSuccessfully = emptiedSuccessfully;
        }

        [Obsolete("Use the other ctor that specifies all property data instead")]
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
        /// <remarks>
        /// This list can be appended to during an event handling operation, generally this is done based on the property data contained in these event args
        /// </remarks>
        public List<string> Files { get; private set; }

        /// <summary>
        /// Gets the list of all property data associated with a content id
        /// </summary>
        public Dictionary<int, IEnumerable<Property>> AllPropertyData { get; private set; }

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