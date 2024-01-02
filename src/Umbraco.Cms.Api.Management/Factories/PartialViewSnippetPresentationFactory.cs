using Umbraco.Cms.Api.Management.ViewModels.PartialView.Snippets;
using Umbraco.Cms.Core.Snippets;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Factories;

public class PartialViewSnippetPresentationFactory : IPartialViewSnippetPresentationFactory
{
    private readonly IShortStringHelper _shortStringHelper;

    public PartialViewSnippetPresentationFactory(IShortStringHelper shortStringHelper)
        => _shortStringHelper = shortStringHelper;

    public PartialViewSnippetItemResponseModel CreateSnippetItemResponseModel(string fileName)
        => new PartialViewSnippetItemResponseModel
        {
            FileName = fileName,
            Name = ParseSnippetName(fileName)
        };

    public PartialViewSnippetResponseModel CreateSnippetResponseModel(PartialViewSnippet snippet)
        => new PartialViewSnippetResponseModel
        {
            FileName = snippet.Name,
            Name = ParseSnippetName(snippet.Name),
            Content = snippet.Content
        };

    private string ParseSnippetName(string fileName)
        => fileName.SplitPascalCasing(_shortStringHelper).ToFirstUpperInvariant();
}
