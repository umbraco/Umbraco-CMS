namespace Umbraco.Cms.Core.IO;

public interface IFileSystemFactory
{
    IPhysicalFileSystem CreatePartialViewFileSystem();

    IPhysicalFileSystem CreateScriptsFileSystem();

    IPhysicalFileSystem CreateMvcViewsFileSystem();

    IPhysicalFileSystem CreateStylesheetFileSystem();


    IPhysicalFileSystem CreateMediaFileSystem();

    IPhysicalFileSystem CreateFileSystem(string rootPath, string rootUrl);
}

