using System;
using System.Runtime.Serialization;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.IO;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Represents a Script file
    /// </summary>
    [Serializable]
    [DataContract(IsReference = true)]
    public class Script : File
    {
        public Script(string path)
            : this(path, (Func<File, string>) null)
        { }

        internal Script(string path, Func<File, string> getFileContent)
            : base(path, getFileContent)
        { }

        [Obsolete("This is no longer used and will be removed from the codebase in future versions")]
        public Script(string path, IContentSection contentConfig)
            : base(path)
        { }

        /// <summary>
        /// Indicates whether the current entity has an identity, which in this case is a path/name.
        /// </summary>
        /// <remarks>
        /// Overrides the default Entity identity check.
        /// </remarks>
        public override bool HasIdentity
        {
            get { return string.IsNullOrEmpty(Path) == false; }
        }
    }
}