using Umbraco.Core.Models.EntityBase;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Defines a File
    /// </summary>
    /// <remarks>Used for Scripts, Stylesheets and Templates</remarks>
    public interface IFile : IAggregateRoot
    {
        /// <summary>
        /// Gets the Name of the File including extension
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the Alias of the File, which is the name without the extension
        /// </summary>
        string Alias { get; }

        /// <summary>
        /// Gets or sets the Path to the File from the root of the file's associated IFileSystem
        /// </summary>
        string Path { get; set; }

        /// <summary>
        /// Gets or sets the Content of a File
        /// </summary>
        string Content { get; set; }

        /// <summary>
        /// Gets or sets the file's virtual path (i.e. the file path relative to the root of the website)
        /// </summary>
        string VirtualPath { get; set; }

        /// <summary>
        /// Boolean indicating whether the file could be validated
        /// </summary>
        /// <returns>True if file is valid, otherwise false</returns>
        bool IsValid();
    }
}