using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Extensions;
using Umbraco.Cms.Core.IO;
using Umbraco.Extensions;
using IHostingEnvironment = Umbraco.Cms.Core.Hosting.IHostingEnvironment;

namespace Umbraco.Cms.Web.Common.FileSystem;

public class FileSystemFactory : IFileSystemFactory
{
    private readonly IIOHelper _ioHelper;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly IHostEnvironment _hostEnvironment;
    private readonly IHostingEnvironment _hostingEnvironment;
    private readonly ILogger<PhysicalFileSystem> _fileSystemLogger;
    private readonly GlobalSettings _globalSettings;

    public FileSystemFactory(
        IIOHelper ioHelper,
        IWebHostEnvironment webHostEnvironment,
        IHostEnvironment hostEnvironment,
        IHostingEnvironment hostingEnvironment,
        ILoggerFactory loggerFactory,
        IOptions<GlobalSettings> globalSettings)
    {
        _ioHelper = ioHelper;
        _webHostEnvironment = webHostEnvironment;
        _hostEnvironment = hostEnvironment;
        _hostingEnvironment = hostingEnvironment;
        _fileSystemLogger = loggerFactory.CreateLogger<PhysicalFileSystem>();
        _globalSettings = globalSettings.Value;
    }

    public IPhysicalFileSystem CreatePartialViewFileSystem() =>
        new PhysicalFileSystem(
            _ioHelper,
            _hostEnvironment,
            _fileSystemLogger,
            _hostEnvironment.MapPathContentRoot(Core.Constants.SystemDirectories.PartialViews),
            _hostingEnvironment.ToAbsolute(Core.Constants.SystemDirectories.PartialViews));

    public IPhysicalFileSystem CreateScriptsFileSystem() =>
        new PhysicalFileSystem(
            _ioHelper,
            _hostEnvironment,
            _fileSystemLogger,
            _webHostEnvironment.MapPathWebRoot(_globalSettings.UmbracoScriptsPath),
            _hostingEnvironment.ToAbsolute(_globalSettings.UmbracoScriptsPath));

    //TODO this is fucked, why do PhysicalFileSystem has a root url? Mvc views cannot be accessed by url! (Comment moved from FileSystems.cs)
    public IPhysicalFileSystem CreateMvcViewsFileSystem() =>
        new PhysicalFileSystem(
            _ioHelper,
            _hostEnvironment,
            _fileSystemLogger,
            _hostEnvironment.MapPathContentRoot(Core.Constants.SystemDirectories.MvcViews),
            _hostingEnvironment.ToAbsolute(Core.Constants.SystemDirectories.MvcViews));

    public IPhysicalFileSystem CreateStylesheetFileSystem() =>
        new PhysicalFileSystem(
            _ioHelper,
            _hostEnvironment,
            _fileSystemLogger,
            _webHostEnvironment.MapPathWebRoot(_globalSettings.UmbracoCssPath),
            _hostingEnvironment.ToAbsolute(_globalSettings.UmbracoCssPath));

    public IPhysicalFileSystem CreateMediaFileSystem()
    {
        var rootPath = Path.IsPathRooted(_globalSettings.UmbracoMediaPhysicalRootPath)
            ? _globalSettings.UmbracoMediaPhysicalRootPath
            : _webHostEnvironment.MapPathWebRoot(_globalSettings.UmbracoMediaPhysicalRootPath);
        return new PhysicalFileSystem(
            _ioHelper,
            _hostEnvironment,
            _fileSystemLogger,
            rootPath,
            _hostingEnvironment.ToAbsolute(_globalSettings.UmbracoMediaPath));
    }

    public IPhysicalFileSystem CreateFileSystem(string rootPath, string rootUrl) =>
        new PhysicalFileSystem(_ioHelper, _hostEnvironment, _fileSystemLogger, rootPath, rootUrl);
}
