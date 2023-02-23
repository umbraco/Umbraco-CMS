using Examine;
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
        RegisterIndex("ContentAPIIndex");
    }

    protected override void PopulateIndexes(IReadOnlyList<IIndex> indexes)
    {
        foreach (var index in indexes)
        {
            var rootNodes = _contentService.GetRootContent();

            index.IndexItems(_contentApiValueSetBuilder.GetValueSets(rootNodes.ToArray()));

            foreach (var root in rootNodes)
            {
                var valueSets = _contentApiValueSetBuilder.GetValueSets(_contentService.GetPagedDescendants(root.Id, 0, Int32.MaxValue, out _).ToArray());
                index.IndexItems(valueSets);
            }
        }
    }
}
