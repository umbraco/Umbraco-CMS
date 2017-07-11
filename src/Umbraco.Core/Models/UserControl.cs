using System;
using System.Runtime.Serialization;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Represents a UserControl file
    /// </summary>
    [Serializable]
    [DataContract(IsReference = true)]
    public class UserControl : File, IUserControl
    {
        public UserControl(string path)
            : this(path, (Func<File, string>)null)
        { }

        internal UserControl(string path, Func<File, string> getFileContent)
            : base(path, getFileContent)
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