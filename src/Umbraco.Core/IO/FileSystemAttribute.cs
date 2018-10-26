using System;

namespace Umbraco.Core.IO
{
    /// <summary>
    /// Decorates a filesystem.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class FileSystemAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FileSystemAttribute"/> class.
        /// </summary>
        /// <param name="alias"></param>
        public FileSystemAttribute(string alias)
        {
            Alias = alias;
        }

        /// <summary>
        /// Gets the alias of the filesystem.
        /// </summary>
        public string Alias { get; }
    }
}
