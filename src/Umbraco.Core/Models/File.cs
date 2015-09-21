using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using Umbraco.Core.IO;
using Umbraco.Core.Models.EntityBase;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Represents an abstract file which provides basic functionality for a File with an Alias and Name
    /// </summary>
    [Serializable]
    [DataContract(IsReference = true)]
    public abstract class File : Entity, IFile
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

        private static readonly PropertyInfo ContentSelector = ExpressionHelper.GetPropertyInfo<File, string>(x => x.Content);
        private static readonly PropertyInfo PathSelector = ExpressionHelper.GetPropertyInfo<File, string>(x => x.Path);
        private string _alias;
        private string _name;

        private static string SanitizePath(string path)
        {
            return path
                .Replace('\\', System.IO.Path.DirectorySeparatorChar)
                .Replace('/', System.IO.Path.DirectorySeparatorChar);
                //.TrimStart(System.IO.Path.DirectorySeparatorChar);
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

                SetPropertyValueAndDetectChanges(o =>
                {
                    _path = SanitizePath(value);
                    return _path;
                }, _path, PathSelector);
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
                SetPropertyValueAndDetectChanges(o =>
                {
                    _content = value ?? string.Empty; // cannot set to null
                    return _content;
                }, _content, ContentSelector);
            }
        }

        /// <summary>
        /// Gets or sets the file's virtual path (i.e. the file path relative to the root of the website)
        /// </summary>
        public string VirtualPath { get; set; }

        [Obsolete("This is no longer used and will be removed from the codebase in future versions")]
        public virtual bool IsValid()
        {
            return true;
        }

        // this exists so that class that manage name and alias differently, eg Template,
        // can implement their own cloning - (though really, not sure it's even needed)
        protected virtual void DeepCloneNameAndAlias(File clone)
        {
            // set fields that have a lazy value, by forcing evaluation of the lazy
            clone._name = Name;
            clone._alias = Alias;
        }

        public override object DeepClone()
        {
            var clone = (File) base.DeepClone();

            // clear fields that were memberwise-cloned and that we don't want to clone
            clone._content = null;

            // turn off change tracking
            clone.DisableChangeTracking();

            // ...
            DeepCloneNameAndAlias(clone);

            // this shouldn't really be needed since we're not tracking
            clone.ResetDirtyProperties(false);

            // re-enable tracking
            clone.EnableChangeTracking();

            return clone;
        }
    }
}