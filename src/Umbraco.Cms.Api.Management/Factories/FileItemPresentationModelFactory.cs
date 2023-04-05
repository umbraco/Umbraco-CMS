using Umbraco.Cms.Api.Management.ViewModels.PartialView.Item;
using Umbraco.Cms.Api.Management.ViewModels.Script.Item;
using Umbraco.Cms.Api.Management.ViewModels.StaticFile.Item;
using Umbraco.Cms.Api.Management.ViewModels.Stylesheet.Item;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.IO;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Factories;

public class FileItemPresentationModelFactory : IFileItemPresentationModelFactory
{
    public IEnumerable<PartialViewItemResponseModel> CreatePartialViewResponseModels(IEnumerable<string> paths, IFileSystem fileSystem) =>
        paths.Select(
            path => new PartialViewItemResponseModel
            {
                Path = path, Name = fileSystem.GetFileName(path), Icon = Constants.Icons.PartialView,
            });

    public IEnumerable<ScriptItemResponseModel> CreateScriptItemResponseModels(IEnumerable<string> paths, IFileSystem fileSystem) =>
        paths.Select(
            path => new ScriptItemResponseModel
            {
                Path = path, Name = fileSystem.GetFileName(path), Icon = Constants.Icons.PartialView,
            });

    public IEnumerable<StaticFileItemResponseModel> CreateStaticFileItemResponseModels(IEnumerable<string> paths, IFileSystem fileSystem) =>
        paths.Select(
            path => new StaticFileItemResponseModel
            {
                Path = path, Name = fileSystem.GetFileName(path), Icon = Constants.Icons.PartialView,
            });

    public IEnumerable<StylesheetItemResponseModel> CreateStylesheetItemResponseModels(IEnumerable<string> paths, IFileSystem fileSystem) =>
        paths.Select(
            path => new StylesheetItemResponseModel
            {
                Path = path, Name = fileSystem.GetFileName(path), Icon = Constants.Icons.PartialView,
            });
}
