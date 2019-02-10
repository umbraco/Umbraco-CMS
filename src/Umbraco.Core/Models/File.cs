﻿using System;
using System.Runtime.Serialization;
using Umbraco.Core.Models.Entities;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Represents an abstract file which provides basic functionality for a File with an Alias and Name
    /// </summary>
    [Serializable]
    [DataContract(IsReference = true)]
    public abstract class File : EntityBase, IFile
    {
        private string _path;
        private string _originalPath;

        // initialize to string.Empty so that it is possible to save a new file,
        // should use the lazyContent ctor to set it to null when loading existing.
        // cannot simply use HasIdentity as some classes (eg Script) override it
        // in a weird way.
        private string _content;
        internal Func<File, string> GetFileContent { get; set; }

        protected File(string path, Func<File, string> getFileContent = null)
        {
            _path = SanitizePath(path);
            _originalPath = _path;
            GetFileContent = getFileContent;
            _content = getFileContent != null ? null : string.Empty;
        }

        private string _alias;
        private string _name;

        private static string SanitizePath(string path)
        {
            return path
                .Replace('\\', System.IO.Path.DirectorySeparatorChar)
                .Replace('/', System.IO.Path.DirectorySeparatorChar);

            //Don't strip the start - this was a bug fixed in 7.3, see ScriptRepositoryTests.PathTests
            //.TrimStart(System.IO.Path.DirectorySeparatorChar)
            //.TrimStart('/');
        }

        /// <summary>
        /// Gets or sets the Name of the File including extension
        /// </summary>
        [DataMember]
        public virtual string Name
        {
            get { return _name ?? (_name = System.IO.Path.GetFileName(Path)); }
        }

        /// <summary>
        /// Gets or sets the Alias of the File, which is the name without the extension
        /// </summary>
        [DataMember]
        public virtual string Alias
        {
            get
            {
                if (_alias == null)
                {
                    var name = System.IO.Path.GetFileName(Path);
                    if (name == null) return string.Empty;
                    var lastIndexOf = name.LastIndexOf(".", StringComparison.InvariantCultureIgnoreCase);
                    _alias = name.Substring(0, lastIndexOf);
                }
                return _alias;
            }
        }

        /// <summary>
        /// Gets or sets the Path to the File from the root of the file's associated IFileSystem
        /// </summary>
        [DataMember]
        public virtual string Path
        {
            get { return _path; }
            set
            {
                //reset
                _alias = null;
                _name = null;

                SetPropertyValueAndDetectChanges(SanitizePath(value), ref _path, nameof(Path));
            }
        }

        /// <summary>
        /// Gets the original path of the file
        /// </summary>
        public string OriginalPath
        {
            get { return _originalPath; }
        }

        /// <summary>
        /// Called to re-set the OriginalPath to the Path
        /// </summary>
        public void ResetOriginalPath()
        {
            _originalPath = _path;
        }

        /// <summary>
        /// Gets or sets the Content of a File
        /// </summary>
        /// <remarks>Marked as DoNotClone, because it should be lazy-reloaded from disk.</remarks>
        [DataMember]
        [DoNotClone]
        public virtual string Content
        {
            get
            {
                if (_content != null)
                    return _content;

                // else, must lazy-load, and ensure it's not null
                if (GetFileContent != null)
                    _content = GetFileContent(this);
                return _content ?? (_content = string.Empty);
            }
            set
            {
                SetPropertyValueAndDetectChanges(
                    value ?? string.Empty, // cannot set to null
                    ref _content, nameof(Content));
            }
        }

        /// <summary>
        /// Gets or sets the file's virtual path (i.e. the file path relative to the root of the website)
        /// </summary>
        public string VirtualPath { get; set; }
        
        // this exists so that class that manage name and alias differently, eg Template,
        // can implement their own cloning - (though really, not sure it's even needed)
        protected virtual void DeepCloneNameAndAlias(File clone)
        {
            // set fields that have a lazy value, by forcing evaluation of the lazy
            clone._name = Name;
            clone._alias = Alias;
        }

        protected override void PerformDeepClone(object clone)
        {
            base.PerformDeepClone(clone);

            var clonedFile = (File)clone;

            // clear fields that were memberwise-cloned and that we don't want to clone
            clonedFile._content = null;

            // ...
            DeepCloneNameAndAlias(clonedFile);
        }
    }
}
