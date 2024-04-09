namespace Umbraco.Cms.Core.Persistence.Repositories;

public interface IFileWithFoldersRepository
{
    void AddFolder(string folderPath);

    void DeleteFolder(string folderPath);

    bool FolderExists(string folderPath);

    bool FolderHasContent(string folderPath);
}
