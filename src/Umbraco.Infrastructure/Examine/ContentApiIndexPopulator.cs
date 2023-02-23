using Examine;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Infrastructure.Examine;

public class ContentApiIndexPopulator : IndexPopulator<IUmbracoContentIndex>
{
    private readonly IContentService _contentService;
    private readonly ContentApiValueSetBuilder _contentApiValueSetBuilder;

    public ContentApiIndexPopulator(IContentService contentService, ContentApiValueSetBuilder contentApiValueSetBuilder)
    {
        _contentService = contentService;
        _contentApiValueSetBuilder = contentApiValueSetBuilder;
        RegisterIndex(Constants.UmbracoIndexes.ContentAPIIndexName);
    }

    protected override void PopulateIndexes(IReadOnlyList<IIndex> indexes)
    {
        foreach (IIndex index in indexes)
        {
            IEnumerable<IContent> rootNodes = _contentService.GetRootContent();

            index.IndexItems(_contentApiValueSetBuilder.GetValueSets(rootNodes.ToArray()));

            foreach (IContent root in rootNodes)
            {
                IEnumerable<ValueSet> valueSets = _contentApiValueSetBuilder.GetValueSets(
                    _contentService.GetPagedDescendants(root.Id, 0, int.MaxValue, out _).ToArray());

                index.IndexItems(valueSets);
            }
        }
    }
}
