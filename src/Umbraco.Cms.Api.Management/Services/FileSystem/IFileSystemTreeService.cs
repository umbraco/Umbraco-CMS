using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core.IO;

namespace Umbraco.Cms.Api.Management.Services.FileSystem;

public interface IFileSystemTreeService
{
    FileSystemTreeItemPresentationModel[] GetAncestorModels(string path, bool includeSelf);

    FileSystemTreeItemPresentationModel[] GetPathViewModels(string path, int skip, int take, out long totalItems);

    FileSystemTreeItemPresentationModel[] GetSiblingsViewModel(string path, int before, int after, out long totalBefore,
        out long totalAfter);

    string[] GetDirectories(string path);

    string[] GetFiles(string path);
}
