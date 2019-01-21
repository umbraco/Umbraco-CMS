namespace Umbraco.Core.IO
{
    /// <summary>
    /// Provides the system filesystems.
    /// </summary>
    public interface IFileSystems
    {
        /// <summary>
        /// Gets the macro partials filesystem.
        /// </summary>
        IFileSystem MacroPartialsFileSystem { get; }

        /// <summary>
        /// Gets the partial views filesystem.
        /// </summary>
        IFileSystem PartialViewsFileSystem { get; }

        /// <summary>
        /// Gets the stylesheets filesystem.
        /// </summary>
        IFileSystem StylesheetsFileSystem { get; }

        /// <summary>
        /// Gets the scripts filesystem.
        /// </summary>
        IFileSystem ScriptsFileSystem { get; }

        /// <summary>
        /// Gets the MVC views filesystem.
        /// </summary>
        IFileSystem MvcViewsFileSystem { get; }
    }
}
