using System;
using System.Runtime.Serialization;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Represents a Partial View file
    /// </summary>
    [Serializable]
    [DataContract(IsReference = true)]
    public class PartialView : File, IPartialView
    {
        [Obsolete("Use the ctor that explicitely sets the view type.")]
        public PartialView(string path)
            : this(PartialViewType.PartialView, path, null)
        { }

        public PartialView(PartialViewType viewType, string path)
            : this(viewType, path, null)
        { }

        internal PartialView(PartialViewType viewType, string path, Func<File, string> getFileContent)
            : base(path, getFileContent)
        {
            ViewType = viewType;
        }

        public PartialViewType ViewType { get; set; }
    }
}