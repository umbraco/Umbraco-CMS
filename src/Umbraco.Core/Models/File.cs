using System;
using System.IO;

namespace Umbraco.Core.Models
{
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
        public virtual string Path { get; set; }

        /// <summary>
        /// Gets or sets the Content of a File
        /// </summary>
        public virtual string Content { get; set; }

        /// <summary>
        /// Boolean indicating whether the file could be validated
        /// </summary>
        /// <returns>True if file is valid, otherwise false</returns>
        public abstract bool IsValid();
    }
}