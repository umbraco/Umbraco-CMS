using Examine;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Infrastructure.Examine;

public class ContentApiIndexPopulator : IndexPopulator
{
    private readonly IContentService _contentService;
    private readonly IContentApiValueSetBuilder _contentValueSetBuilder;

    public ContentApiIndexPopulator(IContentService contentService, IContentApiValueSetBuilder contentValueSetBuilder)
    {
        _contentService = contentService;
        _contentValueSetBuilder = contentValueSetBuilder;
        RegisterIndex(Constants.UmbracoIndexes.ContentAPIIndexName);
    }

    protected override void PopulateIndexes(IReadOnlyList<IIndex> indexes)
    {
        foreach (IIndex index in indexes)
        {
            IEnumerable<IContent> rootNodes = _contentService.GetRootContent();

            index.IndexItems(_contentValueSetBuilder.GetValueSets(rootNodes.ToArray()));

            foreach (IContent root in rootNodes)
            {
                IEnumerable<ValueSet> valueSets = _contentValueSetBuilder.GetValueSets(
                    _contentService.GetPagedDescendants(root.Id, 0, int.MaxValue, out _).ToArray());

                index.IndexItems(valueSets);
            }
        }
    }
}
