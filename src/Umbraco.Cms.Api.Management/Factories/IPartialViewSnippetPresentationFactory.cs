using Umbraco.Cms.Api.Management.ViewModels.PartialView.Snippets;
using Umbraco.Cms.Core.Snippets;

namespace Umbraco.Cms.Api.Management.Factories;

public interface IPartialViewSnippetPresentationFactory
{
    PartialViewSnippetItemResponseModel CreateSnippetItemResponseModel(string fileName);

    PartialViewSnippetResponseModel CreateSnippetResponseModel(PartialViewSnippet snippet);
}
