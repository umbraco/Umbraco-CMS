using System.Diagnostics.CodeAnalysis;

namespace Umbraco.Cms.Api.Management.ViewModels.PartialView.Snippets;

public class SnippetItemResponseModel
{
    public SnippetItemResponseModel()
    {

    }

    [SetsRequiredMembers]
    public SnippetItemResponseModel(string name)
    {
        Name = name;
    }

    public required string Name { get; set; }
}
