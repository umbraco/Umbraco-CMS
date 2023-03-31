using Umbraco.Cms.Api.Management.ViewModels.PartialView.Item;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.IO;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Factories;

public class PartialViewPresentationModelFactory : IPartialViewPresentationModelFactory
{
    private readonly IFileSystem _fileSystem;

    public PartialViewPresentationModelFactory(FileSystems fileSystems) => _fileSystem = fileSystems.PartialViewsFileSystem ?? throw new ArgumentException("Missing partial views file system", nameof(fileSystems));

    public IEnumerable<PartialViewItemResponseModel> Create(IEnumerable<string> paths) => paths.Select(path => CreateResponseModel(path)).WhereNotNull();

    private PartialViewItemResponseModel? CreateResponseModel(string path)
    {
        if (!_fileSystem.FileExists(path))
        {
            return null;
        }

        return new PartialViewItemResponseModel
        {
            Path = path,
            Name = _fileSystem.GetFileName(path),
            Icon = Constants.Icons.PartialView,
        };
    }
}
