namespace Umbraco.Core.IO
{
    public interface ISystemDirectories
    {
        string Bin { get; }
        string Config { get; }
        string Data { get; }
        string TempData { get; }
        string TempFileUploads { get; }
        string TempImageUploads { get; }
        string Install { get; }
        string AppCode { get; }
        string AppPlugins { get; }
        string MvcViews { get; }
        string PartialViews { get; }
        string MacroPartials { get; }
        string Media { get; }
        string Scripts { get; }
        string Css { get; }
        string Umbraco { get; }
        string Packages { get; }
        string Preview { get; }

        /// <summary>
        /// Gets the root path of the application
        /// </summary>
        string Root
        {
            get;
            set; //Only required for unit tests
        }
    }
}
