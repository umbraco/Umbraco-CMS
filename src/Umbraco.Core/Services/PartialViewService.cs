using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Snippets;
using Umbraco.New.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services;

public class PartialViewService : IPartialViewService
{
    private readonly PartialViewSnippetCollection _snippetCollection;

    public PartialViewService(PartialViewSnippetCollection snippetCollection)
    {
        _snippetCollection = snippetCollection;
    }


    public Task<PagedModel<PartialViewSnippet>> GetPartialViewSnippetsAsync(int skip, int take)
    {
        string[] names = _snippetCollection.GetNames().ToArray();
        var total = names.Length;

        IEnumerable<PartialViewSnippet> snippets = names
            .Skip(skip)
            .Take(take)
            .Select(name =>
                new PartialViewSnippet { Name = name, Content = _snippetCollection.GetContentFromName(name) });

        return Task.FromResult(new PagedModel<PartialViewSnippet> { Items = snippets, Total = total });
    }
}
