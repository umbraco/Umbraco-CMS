using Umbraco.Cms.Api.Management.ViewModels.PartialView.Item;
using Umbraco.Cms.Api.Management.ViewModels.Script.Item;
using Umbraco.Cms.Api.Management.ViewModels.StaticFile.Item;
using Umbraco.Cms.Api.Management.ViewModels.Stylesheet.Item;
using Umbraco.Cms.Core.IO;

namespace Umbraco.Cms.Api.Management.Factories;

public interface IFileItemPresentationModelFactory
{
    IEnumerable<PartialViewItemResponseModel> CreatePartialViewResponseModels(IEnumerable<string> path, IFileSystem fileSystem);

    IEnumerable<ScriptItemResponseModel> CreateScriptItemResponseModels(IEnumerable<string> paths, IFileSystem fileSystem);
    IEnumerable<StaticFileItemResponseModel> CreateStaticFileItemResponseModels(IEnumerable<string> paths, IFileSystem fileSystem);
    IEnumerable<StylesheetItemResponseModel> CreateStylesheetItemResponseModels(IEnumerable<string> paths, IFileSystem fileSystem);
}
