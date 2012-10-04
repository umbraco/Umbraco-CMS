using System;
using System.IO;
using System.Runtime.Serialization;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Represents an abstract file which provides basic functionality for a File with an Alias and Name
    /// </summary>
    [Serializable]
    [DataContract(IsReference = true)]
    public abstract class File : IFile
    {
        private string _name;
        private string _alias;

        protected File(string path)
        {
            Path = path;
        }

        /// <summary>
        /// Gets or sets the Name of the File including extension
        /// </summary>
        [DataMember]
        public virtual string Name
        {
            get
            {
                if (!string.IsNullOrEmpty(_name))
                    return _name;

                _name = new FileInfo(Path).Name;
                return _name;
            }
            set { _name = value; }
        }

        /// <summary>
        /// Gets or sets the Alias of the File, which is the name without the extension
        /// </summary>
        [DataMember]
        public virtual string Alias
        {
            get
            {
                if (!string.IsNullOrEmpty(_alias))
                    return _alias;

                var fileInfo = new FileInfo(Path);
                var name = fileInfo.Name;
                int lastIndexOf = name.LastIndexOf(".", StringComparison.InvariantCultureIgnoreCase) + 1;
                _alias = name.Substring(0, lastIndexOf);
                return _alias;
            }
            set { _alias = value; }
        }

        /// <summary>
        /// Gets or sets the Path to the File from the root of the site
        /// </summary>
        [DataMember]
        public virtual string Path { get; set; }

        /// <summary>
        /// Gets or sets the Content of a File
        /// </summary>
        [DataMember]
        public virtual string Content { get; set; }

        /// <summary>
        /// Boolean indicating whether the file could be validated
        /// </summary>
        /// <returns>True if file is valid, otherwise false</returns>
        public abstract bool IsValid();
    }
}