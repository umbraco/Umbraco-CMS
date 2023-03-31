using Umbraco.Cms.Api.Management.ViewModels.PartialView.Item;

namespace Umbraco.Cms.Api.Management.Factories;

public interface IPartialViewPresentationModelFactory
{
    IEnumerable<PartialViewItemResponseModel> Create(IEnumerable<string> path);
}
