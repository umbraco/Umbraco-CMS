using Umbraco.Cms.Api.Management.ViewModels.PartialView.Item;
using Umbraco.Cms.Api.Management.ViewModels.Script.Item;

namespace Umbraco.Cms.Api.Management.Factories;

public interface IFileItemPresentationModelFactory
{
    IEnumerable<PartialViewItemResponseModel> CreatePartialViewResponseModels(IEnumerable<string> path);

    IEnumerable<ScriptItemResponseModel> CreateScriptItemResponseModels(IEnumerable<string> paths);
}
