﻿using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using Umbraco.Core.Models.EntityBase;

namespace Umbraco.Core.Models
{
    internal sealed class Folder : Entity
    {
        public Folder(string folderPath)
        {
            Path = folderPath;
        }

        public string Path { get; set; }
    }

    /// <summary>
    /// Represents an abstract file which provides basic functionality for a File with an Alias and Name
    /// </summary>
    [Serializable]
    [DataContract(IsReference = true)]
    public abstract class File : Entity, IFile
    {
        private string _path;
        private string _content = string.Empty; //initialize to empty string, not null

        protected File(string path)
        {
            _path = path;
        }

        private static readonly PropertyInfo ContentSelector = ExpressionHelper.GetPropertyInfo<File, string>(x => x.Content);
        private static readonly PropertyInfo PathSelector = ExpressionHelper.GetPropertyInfo<File, string>(x => x.Path);
        private string _alias;
        private string _name;

        /// <summary>
        /// Gets or sets the Name of the File including extension
        /// </summary>
        [DataMember]
        public virtual string Name
        {
            get
            {
                if (_name == null)
                {
                    _name = System.IO.Path.GetFileName(Path);
                }
                return _name;
            }
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
        /// Gets or sets the Path to the File from the root of the site
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
                    _path = value;
                    return _path;
                }, _path, PathSelector);
            }
        }

        /// <summary>
        /// Gets or sets the Content of a File
        /// </summary>
        [DataMember]
        public virtual string Content
        {
            get { return _content; }
            set
            {
                SetPropertyValueAndDetectChanges(o =>
                {
                    _content = value;
                    return _content;
                }, _content, ContentSelector);
            }
        }

        /// <summary>
        /// Boolean indicating whether the file could be validated
        /// </summary>
        /// <returns>True if file is valid, otherwise false</returns>
        public abstract bool IsValid();
    }
}