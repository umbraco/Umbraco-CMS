using Umbraco.Cms.Api.Management.ViewModels.Item;
using Umbraco.Cms.Api.Management.ViewModels.PartialView.Item;
using Umbraco.Cms.Api.Management.ViewModels.Script.Item;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.IO;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Factories;

public class FileItemPresentationModelFactory : IFileItemPresentationModelFactory
{
    private readonly IFileSystem _fileSystem;

    public FileItemPresentationModelFactory(FileSystems fileSystems) => _fileSystem = fileSystems.PartialViewsFileSystem ?? throw new ArgumentException("Missing partial views file system", nameof(fileSystems));

    public IEnumerable<PartialViewItemResponseModel> CreatePartialViewResponseModels(IEnumerable<string> paths) => paths.Select(path => CreateResponseModel(path) as PartialViewItemResponseModel).WhereNotNull();
    public IEnumerable<ScriptItemResponseModel> CreateScriptItemResponseModels(IEnumerable<string> paths) => paths.Select(path => CreateResponseModel(path) as ScriptItemResponseModel).WhereNotNull();

    private FileItemResponseModelBase? CreateResponseModel(string path)
    {
        if (!_fileSystem.FileExists(path))
        {
            return null;
        }

        return new FileItemResponseModelBase
        {
            Path = path,
            Name = _fileSystem.GetFileName(path),
            Icon = Constants.Icons.PartialView,
        };
    }
}
